using AutoMapper;

namespace Discount.Grpc.Mapper
{
    public class DiscountProfile : Profile
    {
        public DiscountProfile()
        {
            CreateMap<Entities.Coupon, Protos.CouponModel>().ReverseMap();
        }
    }
}
