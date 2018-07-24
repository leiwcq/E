namespace E.Interface.Web
{
    public interface IServiceGatewayFactory
    {
        IServiceGateway GetServiceGateway(IRequest request);
    }
}