using AutoMapper;
using TobacoBackend.Domain.Models;
using TobacoBackend.DTOs;

namespace TobacoBackend.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Cliente, ClienteDTO>().ReverseMap();
            CreateMap<Venta, VentaDTO>()
                .ForMember(dest => dest.UsuarioCreador, opt => opt.MapFrom(src => src.UsuarioCreador))
                .ForMember(dest => dest.UsuarioAsignado, opt => opt.MapFrom(src => src.UsuarioAsignado))
                .ReverseMap();
            CreateMap<VentaProducto, VentaProductoDTO>().ReverseMap();
            CreateMap<Categoria, CategoriaDTO>().ReverseMap();
            CreateMap<VentaPago, VentaPagoDTO>().ReverseMap();
            CreateMap<PrecioEspecial, PrecioEspecialDTO>().ReverseMap();
            CreateMap<ProductQuantityPrice, ProductQuantityPriceDTO>().ReverseMap();

            // Mapeo personalizado para Producto
            CreateMap<ProductoDTO, Producto>()
                .ForMember(dest => dest.Categoria, opt => opt.Ignore())
                .ForMember(dest => dest.CategoriaId, opt => opt.MapFrom(src => src.CategoriaId))
                .ForMember(dest => dest.QuantityPrices, opt => opt.MapFrom(src => src.QuantityPrices));

            CreateMap<Producto, ProductoDTO>()
                .ForMember(dest => dest.CategoriaId, opt => opt.MapFrom(src => src.CategoriaId))
                .ForMember(dest => dest.CategoriaNombre, opt => opt.MapFrom(src => src.Categoria != null ? src.Categoria.Nombre : string.Empty))
                .ForMember(dest => dest.QuantityPrices, opt => opt.MapFrom(src => src.QuantityPrices));

            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.LastLogin, opt => opt.MapFrom(src => src.LastLogin))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.TipoVendedor, opt => opt.MapFrom(src => src.TipoVendedor))
                .ForMember(dest => dest.Zona, opt => opt.MapFrom(src => src.Zona));

            CreateMap<CreateUserDTO, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.LastLogin, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());

            // Mapeo para Abonos
            CreateMap<Abonos, AbonoDTO>()
                .ForMember(dest => dest.ClienteNombre, opt => opt.MapFrom(src => src.Cliente != null ? src.Cliente.Nombre : string.Empty))
                .ReverseMap()
                .ForMember(dest => dest.Cliente, opt => opt.Ignore());

            // Mapeo para ProductoAFavor
            CreateMap<ProductoAFavor, ProductoAFavorDTO>().ReverseMap();

            // Mapeo para Asistencia
            CreateMap<Asistencia, AsistenciaDTO>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty))
                .ForMember(dest => dest.HorasTrabajadas, opt => opt.MapFrom(src => src.HorasTrabajadas))
                .ReverseMap()
                .ForMember(dest => dest.User, opt => opt.Ignore());
        }
    }
}
