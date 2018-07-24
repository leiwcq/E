using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using E.Common;

namespace E.Extensions //Internalize to avoid conflicts
{
    using Events = RecyclableMemoryStreamManager.Events;

    public static class MemoryStreamFactory
    {
        public static bool UseRecyclableMemoryStream { get; set; }

        public static RecyclableMemoryStreamManager RecyclableInstance = new RecyclableMemoryStreamManager();

        public static MemoryStream GetStream()
        {
            return UseRecyclableMemoryStream
                ? RecyclableInstance.GetStream()
                : new MemoryStream();
        }

        public static MemoryStream GetStream(int capacity)
        {
            return UseRecyclableMemoryStream
                ? RecyclableInstance.GetStream(typeof(MemoryStreamFactory).Name, capacity)
                : new MemoryStream(capacity);
        }

        public static MemoryStream GetStream(byte[] bytes)
        {
            return UseRecyclableMemoryStream
                ? RecyclableInstance.GetStream(typeof(MemoryStreamFactory).Name, bytes, 0, bytes.Length)
                : new MemoryStream(bytes);
        }

        public static MemoryStream GetStream(byte[] bytes, int index, int count)
        {
            return UseRecyclableMemoryStream
                ? RecyclableInstance.GetStream(typeof(MemoryStreamFactory).Name, bytes, index, count)
                : new MemoryStream(bytes, index, count);
        }
    }

    /// <summary>
    /// Manages pools of RecyclableMemoryStream objects.
    /// </summary>
    /// <remarks>
    /// There are two pools managed in here. The small pool contains same-sized buffers that are handed to streams
    /// as they write more data.
    /// 
    /// For scenarios that need to call GetBuffer(), the large pool contains buffers of various sizes, all
    /// multiples of LargeBufferMultiple (1 MB by default). They are split by size to avoid overly-wasteful buffer
    /// usage. There should be far fewer 8 MB buffers than 1 MB buffers, for example.
    /// </remarks>
    public partial class RecyclableMemoryStreamManager
    {
        /// <summary>
        /// Generic delegate for handling events without any arguments.
        /// </summary>
        public delegate void EventHandler();

        /// <summary>
        /// Delegate for handling large buffer discard reports.
        /// </summary>
        /// <param name="reason">Reason the buffer was discarded.</param>
        public delegate void LargeBufferDiscardedEventHandler(Events.MemoryStreamDiscardReason reason);

        /// <summary>
        /// Delegate for handling reports of stream size when streams are allocated
        /// </summary>
        /// <param name="bytes">Bytes allocated.</param>
        public delegate void StreamLengthReportHandler(long bytes);

        /// <summary>
        /// Delegate for handling periodic reporting of memory use statistics.
        /// </summary>
        /// <param name="smallPoolInUseBytes">Bytes currently in use in the small pool.</param>
        /// <param name="smallPoolFreeBytes">Bytes currently free in the small pool.</param>
        /// <param name="largePoolInUseBytes">Bytes currently in use in the large pool.</param>
        /// <param name="largePoolFreeBytes">Bytes currently free in the large pool.</param>
        public delegate void UsageReportEventHandler(
            long smallPoolInUseBytes, long smallPoolFreeBytes, long largePoolInUseBytes, long largePoolFreeBytes);

        public const int DEFAULT_BLOCK_SIZE = 128 * 1024;
        public const int DEFAULT_LARGE_BUFFER_MULTIPLE = 1024 * 1024;
        public const int DEFAULT_MAXIMUM_BUFFER_SIZE = 128 * 1024 * 1024;

        private readonly int _blockSize;
        private readonly long[] _largeBufferFreeSize;
        private readonly long[] _largeBufferInUseSize;

        private readonly int _largeBufferMultiple;

        /// <summary>
        /// pools[0] = 1x largeBufferMultiple buffers
        /// pools[1] = 2x largeBufferMultiple buffers
        /// etc., up to maximumBufferSize
        /// </summary>
        private readonly ConcurrentStack<byte[]>[] _largePools;

        private readonly int _maximumBufferSize;
        private readonly ConcurrentStack<byte[]> _smallPool;

        private long _smallPoolFreeSize;
        private long _smallPoolInUseSize;

        /// <summary>
        /// Initializes the memory manager with the default block/buffer specifications.
        /// </summary>
        public RecyclableMemoryStreamManager()
            : this(DEFAULT_BLOCK_SIZE, DEFAULT_LARGE_BUFFER_MULTIPLE, DEFAULT_MAXIMUM_BUFFER_SIZE)
        { }

        /// <summary>
        /// Initializes the memory manager with the given block requiredSize.
        /// </summary>
        /// <param name="blockSize">Size of each block that is pooled. Must be > 0.</param>
        /// <param name="largeBufferMultiple">Each large buffer will be a multiple of this value.</param>
        /// <param name="maximumBufferSize">Buffers larger than this are not pooled</param>
        /// <exception cref="ArgumentOutOfRangeException">blockSize is not a positive number, or largeBufferMultiple is not a positive number, or maximumBufferSize is less than blockSize.</exception>
        /// <exception cref="ArgumentException">maximumBufferSize is not a multiple of largeBufferMultiple</exception>
        public RecyclableMemoryStreamManager(int blockSize, int largeBufferMultiple, int maximumBufferSize)
        {
            if (blockSize <= 0)
            {
                throw new ArgumentOutOfRangeException("blockSize", blockSize, "blockSize must be a positive number");
            }

            if (largeBufferMultiple <= 0)
            {
                throw new ArgumentOutOfRangeException("largeBufferMultiple",
                                                      "largeBufferMultiple must be a positive number");
            }

            if (maximumBufferSize < blockSize)
            {
                throw new ArgumentOutOfRangeException("maximumBufferSize",
                                                      "maximumBufferSize must be at least blockSize");
            }

            _blockSize = blockSize;
            _largeBufferMultiple = largeBufferMultiple;
            _maximumBufferSize = maximumBufferSize;

            if (!IsLargeBufferMultiple(maximumBufferSize))
            {
                throw new ArgumentException("maximumBufferSize is not a multiple of largeBufferMultiple",
                                            "maximumBufferSize");
            }

            _smallPool = new ConcurrentStack<byte[]>();
            var numLargePools = maximumBufferSize / largeBufferMultiple;

            // +1 to store size of bytes in use that are too large to be pooled
            _largeBufferInUseSize = new long[numLargePools + 1];
            _largeBufferFreeSize = new long[numLargePools];

            _largePools = new ConcurrentStack<byte[]>[numLargePools];

            for (var i = 0; i < _largePools.Length; ++i)
            {
                _largePools[i] = new ConcurrentStack<byte[]>();
            }

            Events.Write.MemoryStreamManagerInitialized(blockSize, largeBufferMultiple, maximumBufferSize);
        }

        /// <summary>
        /// The size of each block. It must be set at creation and cannot be changed.
        /// </summary>
        public int BlockSize => _blockSize;

        /// <summary>
        /// All buffers are multiples of this number. It must be set at creation and cannot be changed.
        /// </summary>
        public int LargeBufferMultiple => _largeBufferMultiple;

        /// <summary>
        /// Gets or sets the maximum buffer size.
        /// </summary>
        /// <remarks>Any buffer that is returned to the pool that is larger than this will be
        /// discarded and garbage collected.</remarks>
        public int MaximumBufferSize => _maximumBufferSize;

        /// <summary>
        /// Number of bytes in small pool not currently in use
        /// </summary>
        public long SmallPoolFreeSize => _smallPoolFreeSize;

        /// <summary>
        /// Number of bytes currently in use by stream from the small pool
        /// </summary>
        public long SmallPoolInUseSize => _smallPoolInUseSize;

        /// <summary>
        /// Number of bytes in large pool not currently in use
        /// </summary>
        public long LargePoolFreeSize => _largeBufferFreeSize.Sum();

        /// <summary>
        /// Number of bytes currently in use by streams from the large pool
        /// </summary>
        public long LargePoolInUseSize => _largeBufferInUseSize.Sum();

        /// <summary>
        /// How many blocks are in the small pool
        /// </summary>
        public long SmallBlocksFree => _smallPool.Count;

        /// <summary>
        /// How many buffers are in the large pool
        /// </summary>
        public long LargeBuffersFree
        {
            get
            {
                long free = 0;
                foreach (var pool in _largePools)
                {
                    free += pool.Count;
                }
                return free;
            }
        }

        /// <summary>
        /// How many bytes of small free blocks to allow before we start dropping
        /// those returned to us.
        /// </summary>
        public long MaximumFreeSmallPoolBytes { get; set; }

        /// <summary>
        /// How many bytes of large free buffers to allow before we start dropping
        /// those returned to us.
        /// </summary>
        public long MaximumFreeLargePoolBytes { get; set; }

        /// <summary>
        /// Maximum stream capacity in bytes. Attempts to set a larger capacity will
        /// result in an exception.
        /// </summary>
        /// <remarks>A value of 0 indicates no limit.</remarks>
        public long MaximumStreamCapacity { get; set; }

        /// <summary>
        /// Whether to save callstacks for stream allocations. This can help in debugging.
        /// It should NEVER be turned on generally in production.
        /// </summary>
        public bool GenerateCallStacks { get; set; }

        /// <summary>
        /// Whether dirty buffers can be immediately returned to the buffer pool. E.g. when GetBuffer() is called on
        /// a stream and creates a single large buffer, if this setting is enabled, the other blocks will be returned
        /// to the buffer pool immediately.
        /// Note when enabling this setting that the user is responsible for ensuring that any buffer previously
        /// retrieved from a stream which is subsequently modified is not used after modification (as it may no longer
        /// be valid).
        /// </summary>
        public bool AggressiveBufferReturn { get; set; }

        /// <summary>
        /// Removes and returns a single block from the pool.
        /// </summary>
        /// <returns>A byte[] array</returns>
        internal byte[] GetBlock()
        {
            if (!_smallPool.TryPop(out var block))
            {
                // We'll add this back to the pool when the stream is disposed
                // (unless our free pool is too large)
                block = new byte[BlockSize];
                Events.Write.MemoryStreamNewBlockCreated(_smallPoolInUseSize);

                BlockCreated?.Invoke();
            }
            else
            {
                Interlocked.Add(ref _smallPoolFreeSize, -BlockSize);
            }

            Interlocked.Add(ref _smallPoolInUseSize, BlockSize);
            return block;
        }

        /// <summary>
        /// Returns a buffer of arbitrary size from the large buffer pool. This buffer
        /// will be at least the requiredSize and always be a multiple of largeBufferMultiple.
        /// </summary>
        /// <param name="requiredSize">The minimum length of the buffer</param>
        /// <param name="tag">The tag of the stream returning this buffer, for logging if necessary.</param>
        /// <returns>A buffer of at least the required size.</returns>
        internal byte[] GetLargeBuffer(int requiredSize, string tag)
        {
            requiredSize = RoundToLargeBufferMultiple(requiredSize);

            var poolIndex = requiredSize / _largeBufferMultiple - 1;

            byte[] buffer;
            if (poolIndex < _largePools.Length)
            {
                if (!_largePools[poolIndex].TryPop(out buffer))
                {
                    buffer = new byte[requiredSize];

                    Events.Write.MemoryStreamNewLargeBufferCreated(requiredSize, LargePoolInUseSize);
                    LargeBufferCreated?.Invoke();
                }
                else
                {
                    Interlocked.Add(ref _largeBufferFreeSize[poolIndex], -buffer.Length);
                }
            }
            else
            {
                // Buffer is too large to pool. They get a new buffer.

                // We still want to track the size, though, and we've reserved a slot
                // in the end of the inuse array for nonpooled bytes in use.
                poolIndex = _largeBufferInUseSize.Length - 1;

                // We still want to round up to reduce heap fragmentation.
                buffer = new byte[requiredSize];
                string callStack = null;
                if (GenerateCallStacks)
                {
                    // Grab the stack -- we want to know who requires such large buffers
                    callStack = PclExport.Instance.GetStackTrace();
                }
                Events.Write.MemoryStreamNonPooledLargeBufferCreated(requiredSize, tag, callStack);

                LargeBufferCreated?.Invoke();
            }

            Interlocked.Add(ref _largeBufferInUseSize[poolIndex], buffer.Length);

            return buffer;
        }

        private int RoundToLargeBufferMultiple(int requiredSize)
        {
            return (requiredSize + LargeBufferMultiple - 1) / LargeBufferMultiple * LargeBufferMultiple;
        }

        private bool IsLargeBufferMultiple(int value)
        {
            return value != 0 && value % LargeBufferMultiple == 0;
        }

        /// <summary>
        /// Returns the buffer to the large pool
        /// </summary>
        /// <param name="buffer">The buffer to return.</param>
        /// <param name="tag">The tag of the stream returning this buffer, for logging if necessary.</param>
        /// <exception cref="ArgumentNullException">buffer is null</exception>
        /// <exception cref="ArgumentException">buffer.Length is not a multiple of LargeBufferMultiple (it did not originate from this pool)</exception>
        internal void ReturnLargeBuffer(byte[] buffer, string tag)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            if (!IsLargeBufferMultiple(buffer.Length))
            {
                throw new ArgumentException(
                    "buffer did not originate from this memory manager. The size is not a multiple of " +
                    LargeBufferMultiple);
            }

            var poolIndex = buffer.Length / _largeBufferMultiple - 1;

            if (poolIndex < _largePools.Length)
            {
                if ((_largePools[poolIndex].Count + 1) * buffer.Length <= MaximumFreeLargePoolBytes ||
                    MaximumFreeLargePoolBytes == 0)
                {
                    _largePools[poolIndex].Push(buffer);
                    Interlocked.Add(ref _largeBufferFreeSize[poolIndex], buffer.Length);
                }
                else
                {
                    Events.Write.MemoryStreamDiscardBuffer(Events.MemoryStreamBufferType.Large, tag,
                                                           Events.MemoryStreamDiscardReason.EnoughFree);

                    LargeBufferDiscarded?.Invoke(Events.MemoryStreamDiscardReason.EnoughFree);
                }
            }
            else
            {
                // This is a non-poolable buffer, but we still want to track its size for inuse
                // analysis. We have space in the inuse array for this.
                poolIndex = _largeBufferInUseSize.Length - 1;

                Events.Write.MemoryStreamDiscardBuffer(Events.MemoryStreamBufferType.Large, tag,
                                                       Events.MemoryStreamDiscardReason.TooLarge);
                LargeBufferDiscarded?.Invoke(Events.MemoryStreamDiscardReason.TooLarge);
            }

            Interlocked.Add(ref _largeBufferInUseSize[poolIndex], -buffer.Length);

            UsageReport?.Invoke(_smallPoolInUseSize, _smallPoolFreeSize, LargePoolInUseSize,
                 LargePoolFreeSize);
        }

        /// <summary>
        /// Returns the blocks to the pool
        /// </summary>
        /// <param name="blocks">Collection of blocks to return to the pool</param>
        /// <param name="tag">The tag of the stream returning these blocks, for logging if necessary.</param>
        /// <exception cref="ArgumentNullException">blocks is null</exception>
        /// <exception cref="ArgumentException">blocks contains buffers that are the wrong size (or null) for this memory manager</exception>
        internal void ReturnBlocks(ICollection<byte[]> blocks, string tag)
        {
            if (blocks == null)
            {
                throw new ArgumentNullException("blocks");
            }

            var bytesToReturn = blocks.Count * BlockSize;
            Interlocked.Add(ref _smallPoolInUseSize, -bytesToReturn);

            foreach (var block in blocks)
            {
                if (block == null || block.Length != BlockSize)
                {
                    throw new ArgumentException("blocks contains buffers that are not BlockSize in length");
                }
            }

            foreach (var block in blocks)
            {
                if (MaximumFreeSmallPoolBytes == 0 || SmallPoolFreeSize < MaximumFreeSmallPoolBytes)
                {
                    Interlocked.Add(ref _smallPoolFreeSize, BlockSize);
                    _smallPool.Push(block);
                }
                else
                {
                    Events.Write.MemoryStreamDiscardBuffer(Events.MemoryStreamBufferType.Small, tag,
                                                           Events.MemoryStreamDiscardReason.EnoughFree);
                    BlockDiscarded?.Invoke();
                    break;
                }
            }

            UsageReport?.Invoke(_smallPoolInUseSize, _smallPoolFreeSize, LargePoolInUseSize,
                 LargePoolFreeSize);
        }

        internal void ReportStreamCreated()
        {
            StreamCreated?.Invoke();
        }

        internal void ReportStreamDisposed()
        {
            StreamDisposed?.Invoke();
        }

        internal void ReportStreamFinalized()
        {
            StreamFinalized?.Invoke();
        }

        internal void ReportStreamLength(long bytes)
        {
            StreamLength?.Invoke(bytes);
        }

        internal void ReportStreamToArray()
        {
            StreamConvertedToArray?.Invoke();
        }

        /// <summary>
        /// Retrieve a new MemoryStream object with no tag and a default initial capacity.
        /// </summary>
        /// <returns>A MemoryStream.</returns>
        public MemoryStream GetStream()
        {
            return new RecyclableMemoryStream(this);
        }

        /// <summary>
        /// Retrieve a new MemoryStream object with the given tag and a default initial capacity.
        /// </summary>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <returns>A MemoryStream.</returns>
        public MemoryStream GetStream(string tag)
        {
            return new RecyclableMemoryStream(this, tag);
        }

        /// <summary>
        /// Retrieve a new MemoryStream object with the given tag and at least the given capacity.
        /// </summary>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="requiredSize">The minimum desired capacity for the stream.</param>
        /// <returns>A MemoryStream.</returns>
        public MemoryStream GetStream(string tag, int requiredSize)
        {
            return new RecyclableMemoryStream(this, tag, requiredSize);
        }

        /// <summary>
        /// Retrieve a new MemoryStream object with the given tag and at least the given capacity, possibly using
        /// a single continugous underlying buffer.
        /// </summary>
        /// <remarks>Retrieving a MemoryStream which provides a single contiguous buffer can be useful in situations
        /// where the initial size is known and it is desirable to avoid copying data between the smaller underlying
        /// buffers to a single large one. This is most helpful when you know that you will always call GetBuffer
        /// on the underlying stream.</remarks>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="requiredSize">The minimum desired capacity for the stream.</param>
        /// <param name="asContiguousBuffer">Whether to attempt to use a single contiguous buffer.</param>
        /// <returns>A MemoryStream.</returns>
        public MemoryStream GetStream(string tag, int requiredSize, bool asContiguousBuffer)
        {
            if (!asContiguousBuffer || requiredSize <= BlockSize)
            {
                return GetStream(tag, requiredSize);
            }

            return new RecyclableMemoryStream(this, tag, requiredSize, GetLargeBuffer(requiredSize, tag));
        }

        /// <summary>
        /// Retrieve a new MemoryStream object with the given tag and with contents copied from the provided
        /// buffer. The provided buffer is not wrapped or used after construction.
        /// </summary>
        /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="buffer">The byte buffer to copy data from.</param>
        /// <param name="offset">The offset from the start of the buffer to copy from.</param>
        /// <param name="count">The number of bytes to copy from the buffer.</param>
        /// <returns>A MemoryStream.</returns>
        //[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public MemoryStream GetStream(string tag, byte[] buffer, int offset, int count)
        {
            var stream = new RecyclableMemoryStream(this, tag, count);
            stream.Write(buffer, offset, count);
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Triggered when a new block is created.
        /// </summary>
        public event EventHandler BlockCreated;

        /// <summary>
        /// Triggered when a new block is created.
        /// </summary>
        public event EventHandler BlockDiscarded;

        /// <summary>
        /// Triggered when a new large buffer is created.
        /// </summary>
        public event EventHandler LargeBufferCreated;

        /// <summary>
        /// Triggered when a new stream is created.
        /// </summary>
        public event EventHandler StreamCreated;

        /// <summary>
        /// Triggered when a stream is disposed.
        /// </summary>
        public event EventHandler StreamDisposed;

        /// <summary>
        /// Triggered when a stream is finalized.
        /// </summary>
        public event EventHandler StreamFinalized;

        /// <summary>
        /// Triggered when a stream is finalized.
        /// </summary>
        public event StreamLengthReportHandler StreamLength;

        /// <summary>
        /// Triggered when a user converts a stream to array.
        /// </summary>
        public event EventHandler StreamConvertedToArray;

        /// <summary>
        /// Triggered when a large buffer is discarded, along with the reason for the discard.
        /// </summary>
        public event LargeBufferDiscardedEventHandler LargeBufferDiscarded;

        /// <summary>
        /// Periodically triggered to report usage statistics.
        /// </summary>
        public event UsageReportEventHandler UsageReport;
    }


    /// <summary>
    /// MemoryStream implementation that deals with pooling and managing memory streams which use potentially large
    /// buffers.
    /// </summary>
    /// <remarks>
    /// This class works in tandem with the RecylableMemoryStreamManager to supply MemoryStream
    /// objects to callers, while avoiding these specific problems:
    /// 1. LOH allocations - since all large buffers are pooled, they will never incur a Gen2 GC
    /// 2. Memory waste - A standard memory stream doubles its size when it runs out of room. This
    /// leads to continual memory growth as each stream approaches the maximum allowed size.
    /// 3. Memory copying - Each time a MemoryStream grows, all the bytes are copied into new buffers.
    /// This implementation only copies the bytes when GetBuffer is called.
    /// 4. Memory fragmentation - By using homogeneous buffer sizes, it ensures that blocks of memory
    /// can be easily reused.
    /// 
    /// The stream is implemented on top of a series of uniformly-sized blocks. As the stream's length grows,
    /// additional blocks are retrieved from the memory manager. It is these blocks that are pooled, not the stream
    /// object itself.
    /// 
    /// The biggest wrinkle in this implementation is when GetBuffer() is called. This requires a single 
    /// contiguous buffer. If only a single block is in use, then that block is returned. If multiple blocks 
    /// are in use, we retrieve a larger buffer from the memory manager. These large buffers are also pooled, 
    /// split by size--they are multiples of a chunk size (1 MB by default).
    /// 
    /// Once a large buffer is assigned to the stream the blocks are NEVER again used for this stream. All operations take place on the 
    /// large buffer. The large buffer can be replaced by a larger buffer from the pool as needed. All blocks and large buffers 
    /// are maintained in the stream until the stream is disposed (unless AggressiveBufferReturn is enabled in the stream manager).
    /// 
    /// </remarks>
    public sealed class RecyclableMemoryStream : MemoryStream
    {
        private const long MAX_STREAM_LENGTH = Int32.MaxValue;

        /// <summary>
        /// All of these blocks must be the same size
        /// </summary>
        private readonly List<byte[]> _blocks = new List<byte[]>(1);

        /// <summary>
        /// This is only set by GetBuffer() if the necessary buffer is larger than a single block size, or on
        /// construction if the caller immediately requests a single large buffer.
        /// </summary>
        /// <remarks>If this field is non-null, it contains the concatenation of the bytes found in the individual
        /// blocks. Once it is created, this (or a larger) largeBuffer will be used for the life of the stream.
        /// </remarks>
        private byte[] _largeBuffer;

        /// <summary>
        /// This list is used to store buffers once they're replaced by something larger.
        /// This is for the cases where you have users of this class that may hold onto the buffers longer
        /// than they should and you want to prevent race conditions which could corrupt the data.
        /// </summary>
        private List<byte[]> _dirtyBuffers;

        private readonly Guid _id;
        /// <summary>
        /// Unique identifier for this stream across it's entire lifetime
        /// </summary>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        internal Guid Id { get { CheckDisposed(); return _id; } }

        private readonly string _tag;
        /// <summary>
        /// A temporary identifier for the current usage of this stream.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        internal string Tag { get { CheckDisposed(); return _tag; } }

        private readonly RecyclableMemoryStreamManager _memoryManager;

        /// <summary>
        /// Gets the memory manager being used by this stream.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        internal RecyclableMemoryStreamManager MemoryManager
        {
            get
            {
                CheckDisposed();
                return _memoryManager;
            }
        }

        private bool _disposed;

        private readonly string _allocationStack;
        private string _disposeStack;

        /// <summary>
        /// Callstack of the constructor. It is only set if MemoryManager.GenerateCallStacks is true,
        /// which should only be in debugging situations.
        /// </summary>
        internal string AllocationStack => _allocationStack;

        /// <summary>
        /// Callstack of the Dispose call. It is only set if MemoryManager.GenerateCallStacks is true,
        /// which should only be in debugging situations.
        /// </summary>
        internal string DisposeStack => _disposeStack;

        /// <summary>
        /// This buffer exists so that WriteByte can forward all of its calls to Write
        /// without creating a new byte[] buffer on every call.
        /// </summary>
        private readonly byte[] _byteBuffer = new byte[1];

        #region Constructors
        /// <inheritdoc />
        /// <summary>
        /// Allocate a new RecyclableMemoryStream object.
        /// </summary>
        /// <param name="memoryManager">The memory manager</param>
        public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager)
            : this(memoryManager, null, 0, null)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Allocate a new RecyclableMemoryStream object
        /// </summary>
        /// <param name="memoryManager">The memory manager</param>
        /// <param name="tag">A string identifying this stream for logging and debugging purposes</param>
        public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, string tag)
            : this(memoryManager, tag, 0, null)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Allocate a new RecyclableMemoryStream object
        /// </summary>
        /// <param name="memoryManager">The memory manager</param>
        /// <param name="tag">A string identifying this stream for logging and debugging purposes</param>
        /// <param name="requestedSize">The initial requested size to prevent future allocations</param>
        public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, string tag, int requestedSize)
            : this(memoryManager, tag, requestedSize, null)
        {
        }

        /// <summary>
        /// Allocate a new RecyclableMemoryStream object
        /// </summary>
        /// <param name="memoryManager">The memory manager</param>
        /// <param name="tag">A string identifying this stream for logging and debugging purposes</param>
        /// <param name="requestedSize">The initial requested size to prevent future allocations</param>
        /// <param name="initialLargeBuffer">An initial buffer to use. This buffer will be owned by the stream and returned to the memory manager upon Dispose.</param>
        internal RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, string tag, int requestedSize,
                                      byte[] initialLargeBuffer)
        {
            _memoryManager = memoryManager;
            _id = Guid.NewGuid();
            _tag = tag;

            if (requestedSize < memoryManager.BlockSize)
            {
                requestedSize = memoryManager.BlockSize;
            }

            if (initialLargeBuffer == null)
            {
                EnsureCapacity(requestedSize);
            }
            else
            {
                _largeBuffer = initialLargeBuffer;
            }

            _disposed = false;

            if (_memoryManager.GenerateCallStacks)
            {
                _allocationStack = PclExport.Instance.GetStackTrace();
            }

            Events.Write.MemoryStreamCreated(_id, _tag, requestedSize);
            _memoryManager.ReportStreamCreated();
        }
        #endregion

        #region Dispose and Finalize
        ~RecyclableMemoryStream()
        {
            Dispose(false);
        }

        /// <summary>
        /// Returns the memory used by this stream back to the pool.
        /// </summary>
        /// <param name="disposing">Whether we're disposing (true), or being called by the finalizer (false)</param>
        /// <remarks>This method is not thread safe and it may not be called more than once.</remarks>
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly", Justification = "We have different disposal semantics, so SuppressFinalize is in a different spot.")]
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                string doubleDisposeStack = null;
                if (_memoryManager.GenerateCallStacks)
                {
                    doubleDisposeStack = PclExport.Instance.GetStackTrace();
                }

                Events.Write.MemoryStreamDoubleDispose(_id, _tag, _allocationStack, _disposeStack, doubleDisposeStack);
                return;
            }

            Events.Write.MemoryStreamDisposed(_id, _tag);

            if (_memoryManager.GenerateCallStacks)
            {
                _disposeStack = PclExport.Instance.GetStackTrace();
            }

            if (disposing)
            {
                // Once this flag is set, we can't access any properties -- use fields directly
                _disposed = true;

                _memoryManager.ReportStreamDisposed();

                GC.SuppressFinalize(this);
            }
            else
            {
                // We're being finalized.

                Events.Write.MemoryStreamFinalized(_id, _tag, _allocationStack);

                if (AppDomain.CurrentDomain.IsFinalizingForUnload())
                {
                    // If we're being finalized because of a shutdown, don't go any further.
                    // We have no idea what's already been cleaned up. Triggering events may cause
                    // a crash.
                    base.Dispose(disposing);
                    return;
                }

                _memoryManager.ReportStreamFinalized();
            }

            _memoryManager.ReportStreamLength(_length);

            if (_largeBuffer != null)
            {
                _memoryManager.ReturnLargeBuffer(_largeBuffer, _tag);
            }

            if (_dirtyBuffers != null)
            {
                foreach (var buffer in _dirtyBuffers)
                {
                    _memoryManager.ReturnLargeBuffer(buffer, _tag);
                }
            }

            _memoryManager.ReturnBlocks(_blocks, _tag);

            base.Dispose(disposing);
        }

        /// <summary>
        /// Equivalent to Dispose
        /// </summary>
        public override void Close()
        {
            Dispose(true);
        }
        #endregion

        #region MemoryStream overrides
        /// <summary>
        /// Gets or sets the capacity
        /// </summary>
        /// <remarks>Capacity is always in multiples of the memory manager's block size, unless
        /// the large buffer is in use.  Capacity never decreases during a stream's lifetime. 
        /// Explicitly setting the capacity to a lower value than the current value will have no effect. 
        /// This is because the buffers are all pooled by chunks and there's little reason to 
        /// allow stream truncation.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        public override int Capacity
        {
            get
            {
                CheckDisposed();
                if (_largeBuffer != null)
                {
                    return _largeBuffer.Length;
                }

                if (_blocks.Count > 0)
                {
                    return _blocks.Count * _memoryManager.BlockSize;
                }
                return 0;
            }
            set
            {
                CheckDisposed();
                EnsureCapacity(value);
            }
        }

        private int _length;

        /// <summary>
        /// Gets the number of bytes written to this stream.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        public override long Length
        {
            get
            {
                CheckDisposed();
                return _length;
            }
        }

        private int _position;

        /// <summary>
        /// Gets the current position in the stream
        /// </summary>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        public override long Position
        {
            get
            {
                CheckDisposed();
                return _position;
            }
            set
            {
                CheckDisposed();
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "value must be non-negative");
                }

                if (value > MAX_STREAM_LENGTH)
                {
                    throw new ArgumentOutOfRangeException("value", "value cannot be more than " + MAX_STREAM_LENGTH);
                }

                _position = (int)value;
            }
        }

        /// <summary>
        /// Whether the stream can currently read
        /// </summary>
        public override bool CanRead => !_disposed;

        /// <summary>
        /// Whether the stream can currently seek
        /// </summary>
        public override bool CanSeek => !_disposed;

        /// <summary>
        /// Always false
        /// </summary>
        public override bool CanTimeout => false;

        /// <summary>
        /// Whether the stream can currently write
        /// </summary>
        public override bool CanWrite => !_disposed;

        /// <summary>
        /// Returns a single buffer containing the contents of the stream.
        /// The buffer may be longer than the stream length.
        /// </summary>
        /// <returns>A byte[] buffer</returns>
        /// <remarks>IMPORTANT: Doing a Write() after calling GetBuffer() invalidates the buffer. The old buffer is held onto
        /// until Dispose is called, but the next time GetBuffer() is called, a new buffer from the pool will be required.</remarks>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        public override byte[] GetBuffer()
        {
            CheckDisposed();

            if (_largeBuffer != null)
            {
                return _largeBuffer;
            }

            if (_blocks.Count == 1)
            {
                return _blocks[0];
            }

            // Buffer needs to reflect the capacity, not the length, because
            // it's possible that people will manipulate the buffer directly
            // and set the length afterward. Capacity sets the expectation
            // for the size of the buffer.
            var newBuffer = _memoryManager.GetLargeBuffer(Capacity, _tag);

            // InternalRead will check for existence of largeBuffer, so make sure we
            // don't set it until after we've copied the data.
            InternalRead(newBuffer, 0, _length, 0);
            _largeBuffer = newBuffer;

            if (_blocks.Count > 0 && _memoryManager.AggressiveBufferReturn)
            {
                _memoryManager.ReturnBlocks(_blocks, _tag);
                _blocks.Clear();
            }

            return _largeBuffer;
        }

        /// <summary>
        /// Returns a new array with a copy of the buffer's contents. You should almost certainly be using GetBuffer combined with the Length to 
        /// access the bytes in this stream. Calling ToArray will destroy the benefits of pooled buffers, but it is included
        /// for the sake of completeness.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        [Obsolete("This method has degraded performance vs. GetBuffer and should be avoided.")]
        public override byte[] ToArray()
        {
            CheckDisposed();
            var newBuffer = new byte[Length];

            InternalRead(newBuffer, 0, _length, 0);
            string stack = _memoryManager.GenerateCallStacks ? PclExport.Instance.GetStackTrace() : null;
            Events.Write.MemoryStreamToArray(_id, _tag, stack, 0);
            _memoryManager.ReportStreamToArray();

            return newBuffer;
        }

        /// <summary>
        /// Reads from the current position into the provided buffer
        /// </summary>
        /// <param name="buffer">Destination buffer</param>
        /// <param name="offset">Offset into buffer at which to start placing the read bytes.</param>
        /// <param name="count">Number of bytes to read.</param>
        /// <returns>The number of bytes read</returns>
        /// <exception cref="ArgumentNullException">buffer is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">offset or count is less than 0</exception>
        /// <exception cref="ArgumentException">offset subtracted from the buffer length is less than count</exception>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            CheckDisposed();
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", "offset cannot be negative");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "count cannot be negative");
            }

            if (offset + count > buffer.Length)
            {
                throw new ArgumentException("buffer length must be at least offset + count");
            }

            int amountRead = InternalRead(buffer, offset, count, _position);
            _position += amountRead;
            return amountRead;
        }

        /// <summary>
        /// Writes the buffer to the stream
        /// </summary>
        /// <param name="buffer">Source buffer</param>
        /// <param name="offset">Start position</param>
        /// <param name="count">Number of bytes to write</param>
        /// <exception cref="ArgumentNullException">buffer is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">offset or count is negative</exception>
        /// <exception cref="ArgumentException">buffer.Length - offset is not less than count</exception>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            CheckDisposed();
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", offset, "Offset must be in the range of 0 - buffer.Length-1");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", count, "count must be non-negative");
            }

            if (count + offset > buffer.Length)
            {
                throw new ArgumentException("count must be greater than buffer.Length - offset");
            }

            int blockSize = _memoryManager.BlockSize;
            long end = (long)_position + count;
            // Check for overflow
            if (end > MAX_STREAM_LENGTH)
            {
                throw new IOException("Maximum capacity exceeded");
            }

            long requiredBuffers = (end + blockSize - 1) / blockSize;

            if (requiredBuffers * blockSize > MAX_STREAM_LENGTH)
            {
                throw new IOException("Maximum capacity exceeded");
            }

            EnsureCapacity((int)end);

            if (_largeBuffer == null)
            {
                int bytesRemaining = count;
                int bytesWritten = 0;
                var blockAndOffset = GetBlockAndRelativeOffset(_position);

                while (bytesRemaining > 0)
                {
                    byte[] currentBlock = _blocks[blockAndOffset.Block];
                    int remainingInBlock = blockSize - blockAndOffset.Offset;
                    int amountToWriteInBlock = Math.Min(remainingInBlock, bytesRemaining);

                    Buffer.BlockCopy(buffer, offset + bytesWritten, currentBlock, blockAndOffset.Offset, amountToWriteInBlock);

                    bytesRemaining -= amountToWriteInBlock;
                    bytesWritten += amountToWriteInBlock;

                    ++blockAndOffset.Block;
                    blockAndOffset.Offset = 0;
                }
            }
            else
            {
                Buffer.BlockCopy(buffer, offset, _largeBuffer, _position, count);
            }
            _position = (int)end;
            _length = Math.Max(_position, _length);
        }

        /// <summary>
        /// Returns a useful string for debugging. This should not normally be called in actual production code.
        /// </summary>
        public override string ToString()
        {
            return string.Format("Id = {0}, Tag = {1}, Length = {2:N0} bytes", Id, Tag, Length);
        }

        /// <summary>
        /// Writes a single byte to the current position in the stream.
        /// </summary>
        /// <param name="value">byte value to write</param>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        public override void WriteByte(byte value)
        {
            CheckDisposed();
            _byteBuffer[0] = value;
            Write(_byteBuffer, 0, 1);
        }

        /// <summary>
        /// Reads a single byte from the current position in the stream.
        /// </summary>
        /// <returns>The byte at the current position, or -1 if the position is at the end of the stream.</returns>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        public override int ReadByte()
        {
            CheckDisposed();
            if (_position == _length)
            {
                return -1;
            }
            byte value;
            if (_largeBuffer == null)
            {
                var blockAndOffset = GetBlockAndRelativeOffset(_position);
                value = _blocks[blockAndOffset.Block][blockAndOffset.Offset];
            }
            else
            {
                value = _largeBuffer[_position];
            }
            _position++;
            return value;
        }

        /// <summary>
        /// Sets the length of the stream
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">value is negative or larger than MaxStreamLength</exception>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        public override void SetLength(long value)
        {
            CheckDisposed();
            if (value < 0 || value > MAX_STREAM_LENGTH)
            {
                throw new ArgumentOutOfRangeException("value", "value must be non-negative and at most " + MAX_STREAM_LENGTH);
            }

            EnsureCapacity((int)value);

            _length = (int)value;
            if (_position > value)
            {
                _position = (int)value;
            }
        }

        /// <summary>
        /// Sets the position to the offset from the seek location
        /// </summary>
        /// <param name="offset">How many bytes to move</param>
        /// <param name="loc">From where</param>
        /// <returns>The new position</returns>
        /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
        /// <exception cref="ArgumentOutOfRangeException">offset is larger than MaxStreamLength</exception>
        /// <exception cref="ArgumentException">Invalid seek origin</exception>
        /// <exception cref="IOException">Attempt to set negative position</exception>
        public override long Seek(long offset, SeekOrigin loc)
        {
            CheckDisposed();
            if (offset > MAX_STREAM_LENGTH)
            {
                throw new ArgumentOutOfRangeException("offset", "offset cannot be larger than " + MAX_STREAM_LENGTH);
            }

            int newPosition;
            switch (loc)
            {
                case SeekOrigin.Begin:
                    newPosition = (int)offset;
                    break;
                case SeekOrigin.Current:
                    newPosition = (int)offset + _position;
                    break;
                case SeekOrigin.End:
                    newPosition = (int)offset + _length;
                    break;
                default:
                    throw new ArgumentException("Invalid seek origin", "loc");
            }
            if (newPosition < 0)
            {
                throw new IOException("Seek before beginning");
            }
            _position = newPosition;
            return _position;
        }

        /// <summary>
        /// Synchronously writes this stream's bytes to the parameter stream.
        /// </summary>
        /// <param name="stream">Destination stream</param>
        /// <remarks>Important: This does a synchronous write, which may not be desired in some situations</remarks>
        public override void WriteTo(Stream stream)
        {
            CheckDisposed();
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (_largeBuffer == null)
            {
                int currentBlock = 0;
                int bytesRemaining = _length;

                while (bytesRemaining > 0)
                {
                    int amountToCopy = Math.Min(_blocks[currentBlock].Length, bytesRemaining);
                    stream.Write(_blocks[currentBlock], 0, amountToCopy);

                    bytesRemaining -= amountToCopy;

                    ++currentBlock;
                }
            }
            else
            {
                stream.Write(_largeBuffer, 0, _length);
            }
        }
        #endregion

        #region Helper Methods
        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(string.Format("The stream with Id {0} and Tag {1} is disposed.", _id, _tag));
            }
        }

        private int InternalRead(byte[] buffer, int offset, int count, int fromPosition)
        {
            if (_length - fromPosition <= 0)
            {
                return 0;
            }
            if (_largeBuffer == null)
            {
                var blockAndOffset = GetBlockAndRelativeOffset(fromPosition);
                int bytesWritten = 0;
                int bytesRemaining = Math.Min(count, _length - fromPosition);

                while (bytesRemaining > 0)
                {
                    int amountToCopy = Math.Min(_blocks[blockAndOffset.Block].Length - blockAndOffset.Offset, bytesRemaining);
                    Buffer.BlockCopy(_blocks[blockAndOffset.Block], blockAndOffset.Offset, buffer, bytesWritten + offset, amountToCopy);

                    bytesWritten += amountToCopy;
                    bytesRemaining -= amountToCopy;

                    ++blockAndOffset.Block;
                    blockAndOffset.Offset = 0;
                }
                return bytesWritten;
            }
            else
            {
                int amountToCopy = Math.Min(count, _length - fromPosition);
                Buffer.BlockCopy(_largeBuffer, fromPosition, buffer, offset, amountToCopy);
                return amountToCopy;
            }
        }

        private struct BlockAndOffset
        {
            public int Block;
            public int Offset;

            public BlockAndOffset(int block, int offset)
            {
                Block = block;
                Offset = offset;
            }
        }

        private BlockAndOffset GetBlockAndRelativeOffset(int offset)
        {
            var blockSize = _memoryManager.BlockSize;
            return new BlockAndOffset(offset / blockSize, offset % blockSize);
        }

        private void EnsureCapacity(int newCapacity)
        {
            if (newCapacity > _memoryManager.MaximumStreamCapacity && _memoryManager.MaximumStreamCapacity > 0)
            {
                Events.Write.MemoryStreamOverCapacity(newCapacity, _memoryManager.MaximumStreamCapacity, _tag, _allocationStack);
                throw new InvalidOperationException("Requested capacity is too large: " + newCapacity + ". Limit is " + _memoryManager.MaximumStreamCapacity);
            }

            if (_largeBuffer != null)
            {
                if (newCapacity > _largeBuffer.Length)
                {
                    var newBuffer = _memoryManager.GetLargeBuffer(newCapacity, _tag);
                    InternalRead(newBuffer, 0, _length, 0);
                    ReleaseLargeBuffer();
                    _largeBuffer = newBuffer;
                }
            }
            else
            {
                while (Capacity < newCapacity)
                {
                    _blocks.Add(_memoryManager.GetBlock());
                }
            }
        }

        /// <summary>
        /// Release the large buffer (either stores it for eventual release or returns it immediately).
        /// </summary>
        private void ReleaseLargeBuffer()
        {
            if (_memoryManager.AggressiveBufferReturn)
            {
                _memoryManager.ReturnLargeBuffer(_largeBuffer, _tag);
            }
            else
            {
                if (_dirtyBuffers == null)
                {
                    // We most likely will only ever need space for one
                    _dirtyBuffers = new List<byte[]>(1);
                }
                _dirtyBuffers.Add(_largeBuffer);
            }

            _largeBuffer = null;
        }
        #endregion
    }

    //Avoid taking on an extra dep
    public sealed partial class RecyclableMemoryStreamManager
    {
        //[EventSource(Name = "Microsoft-IO-RecyclableMemoryStream", Guid = "{B80CD4E4-890E-468D-9CBA-90EB7C82DFC7}")]
        public sealed class Events// : EventSource
        {
            public static Events Write = new Events();

            public enum MemoryStreamBufferType
            {
                Small,
                Large
            }

            public enum MemoryStreamDiscardReason
            {
                TooLarge,
                EnoughFree
            }

            //[Event(1, Level = EventLevel.Verbose)]
            public void MemoryStreamCreated(Guid guid, string tag, int requestedSize)
            {
                //if (this.IsEnabled(EventLevel.Verbose, EventKeywords.None))
                //{
                //    WriteEvent(1, guid, tag ?? string.Empty, requestedSize);
                //}
            }

            //[Event(2, Level = EventLevel.Verbose)]
            public void MemoryStreamDisposed(Guid guid, string tag)
            {
                //if (this.IsEnabled(EventLevel.Verbose, EventKeywords.None))
                //{
                //    WriteEvent(2, guid, tag ?? string.Empty);
                //}
            }

            //[Event(3, Level = EventLevel.Critical)]
            public void MemoryStreamDoubleDispose(Guid guid, string tag, string allocationStack, string disposeStack1,
                                                  string disposeStack2)
            {
                //if (this.IsEnabled())
                //{
                //    this.WriteEvent(3, guid, tag ?? string.Empty, allocationStack ?? string.Empty,
                //                    disposeStack1 ?? string.Empty, disposeStack2 ?? string.Empty);
                //}
            }

            //[Event(4, Level = EventLevel.Error)]
            public void MemoryStreamFinalized(Guid guid, string tag, string allocationStack)
            {
                //if (this.IsEnabled())
                //{
                //    WriteEvent(4, guid, tag ?? string.Empty, allocationStack ?? string.Empty);
                //}
            }

            //[Event(5, Level = EventLevel.Verbose)]
            public void MemoryStreamToArray(Guid guid, string tag, string stack, int size)
            {
                //if (this.IsEnabled(EventLevel.Verbose, EventKeywords.None))
                //{
                //    WriteEvent(5, guid, tag ?? string.Empty, stack ?? string.Empty, size);
                //}
            }

            //[Event(6, Level = EventLevel.Informational)]
            public void MemoryStreamManagerInitialized(int blockSize, int largeBufferMultiple, int maximumBufferSize)
            {
                //if (this.IsEnabled())
                //{
                //    WriteEvent(6, blockSize, largeBufferMultiple, maximumBufferSize);
                //}
            }

            //[Event(7, Level = EventLevel.Verbose)]
            public void MemoryStreamNewBlockCreated(long smallPoolInUseBytes)
            {
                //if (this.IsEnabled(EventLevel.Verbose, EventKeywords.None))
                //{
                //    WriteEvent(7, smallPoolInUseBytes);
                //}
            }

            //[Event(8, Level = EventLevel.Verbose)]
            public void MemoryStreamNewLargeBufferCreated(int requiredSize, long largePoolInUseBytes)
            {
                //if (this.IsEnabled(EventLevel.Verbose, EventKeywords.None))
                //{
                //    WriteEvent(8, requiredSize, largePoolInUseBytes);
                //}
            }

            //[Event(9, Level = EventLevel.Verbose)]
            public void MemoryStreamNonPooledLargeBufferCreated(int requiredSize, string tag, string allocationStack)
            {
                //if (this.IsEnabled(EventLevel.Verbose, EventKeywords.None))
                //{
                //    WriteEvent(9, requiredSize, tag ?? string.Empty, allocationStack ?? string.Empty);
                //}
            }

            //[Event(10, Level = EventLevel.Warning)]
            public void MemoryStreamDiscardBuffer(MemoryStreamBufferType bufferType, string tag,
                                                  MemoryStreamDiscardReason reason)
            {
                //if (this.IsEnabled())
                //{
                //    WriteEvent(10, bufferType, tag ?? string.Empty, reason);
                //}
            }

            //[Event(11, Level = EventLevel.Error)]
            public void MemoryStreamOverCapacity(int requestedCapacity, long maxCapacity, string tag,
                                                 string allocationStack)
            {
                //if (this.IsEnabled())
                //{
                //    WriteEvent(11, requestedCapacity, maxCapacity, tag ?? string.Empty, allocationStack ?? string.Empty);
                //}
            }
        }
    }
}
