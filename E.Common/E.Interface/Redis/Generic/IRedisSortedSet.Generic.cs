using System.Collections.Generic;
using E.Interface.Model;

namespace E.Interface.Redis.Generic
{
    public interface IRedisSortedSet<T> : ICollection<T>, IHasStringId
    {
        T PopItemWithHighestScore();
        T PopItemWithLowestScore();
        double IncrementItem(T item, double incrementBy);
        int IndexOf(T item);
        long IndexOfDescending(T item);
        List<T> GetAll();
        List<T> GetAllDescending();
        List<T> GetRange(int fromRank, int toRank);
        List<T> GetRangeByLowestScore(double fromScore, double toScore);
        List<T> GetRangeByLowestScore(double fromScore, double toScore, int? skip, int? take);
        List<T> GetRangeByHighestScore(double fromScore, double toScore);
        List<T> GetRangeByHighestScore(double fromScore, double toScore, int? skip, int? take);
        long RemoveRange(int minRank, int maxRank);
        long RemoveRangeByScore(double fromScore, double toScore);
        double GetItemScore(T item);
        long PopulateWithIntersectOf(params IRedisSortedSet<T>[] setIds);
        long PopulateWithUnionOf(params IRedisSortedSet<T>[] setIds);
    }
}