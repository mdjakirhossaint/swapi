using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoowGoodWeb.Utilities
{
    public static class MapperCommonService
    {
        // Static method for mapping lists of objects
        public static async Task<List<TDestination>> MapList<TSource, TDestination>(IEnumerable<TSource> sourceList)
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TSource, TDestination>();
            });

            var mapper = configuration.CreateMapper();

            return await Task.Run(() => mapper.Map<List<TDestination>>(sourceList));
        }

        // Static method for mapping a single object and returning it as a list
        public static async Task<List<TDestination>> MapSingleToList<TSource, TDestination>(TSource source)
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TSource, TDestination>();
            });

            var mapper = configuration.CreateMapper();

            return await Task.Run(() =>
            {
                var destination = mapper.Map<TDestination>(source);
                return new List<TDestination> { destination };
            });
        }

        // Static method for mapping a single object
        public static async Task<TDestination> MapSingle<TSource, TDestination>(TSource source)
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TSource, TDestination>();
            });

            var mapper = configuration.CreateMapper();

            return await Task.Run(() => mapper.Map<TDestination>(source));
        }
    }
}
