using AutoMapper;
using Test.Models;
using Test.Models.Dto;

namespace Test
{
    public class MappingConfg
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
                config.CreateMap<Post, PostDto>().ReverseMap();
                config.CreateMap<ResponseDto, ResponseDto>().ReverseMap();
            });

            return mappingConfig;
        }
    }
}