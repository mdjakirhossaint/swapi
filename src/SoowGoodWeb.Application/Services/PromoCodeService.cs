//using Microsoft.Extensions.Logging;
using SoowGoodWeb.DtoModels;
using SoowGoodWeb.Enums;
using SoowGoodWeb.InputDto;
using SoowGoodWeb.Interfaces;
using SoowGoodWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Uow;

namespace SoowGoodWeb.Services
{
    public class PromoCodeService : SoowGoodWebAppService, IPromoCodeService
    {
        private readonly IRepository<PromoCode> _promoCodeRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        public PromoCodeService(IRepository<PromoCode> promoCodeRepository,IUnitOfWorkManager unitOfWorkManager)
        {
            _promoCodeRepository = promoCodeRepository;
            _unitOfWorkManager = unitOfWorkManager;
        }
        public async Task<PromoCodeDto> CreateAsync(PromoCodeInputDto input)
        {

            var newEntity = ObjectMapper.Map<PromoCodeInputDto, PromoCode>(input);

            var promoCode = await _promoCodeRepository.InsertAsync(newEntity);

            await _unitOfWorkManager.Current.SaveChangesAsync();

            return ObjectMapper.Map<PromoCode, PromoCodeDto>(promoCode);
        }

        public async Task<PromoCodeDto> GetAsync(int id)
        {
            var item = await _promoCodeRepository.WithDetailsAsync();

            var code = item.FirstOrDefault(item => item.Id == id);

            var result = code != null ? ObjectMapper.Map<PromoCode, PromoCodeDto>(code) : null;

            return result;
        }

        public async Task<PromoCodeDto> GetByNameAsync(string name)
        {
            var item = await _promoCodeRepository.WithDetailsAsync();

            var code = item.FirstOrDefault(item => item.PromoCodeName == name);

            var result = code != null ? ObjectMapper.Map<PromoCode, PromoCodeDto>(code) : null;

            return result;
        }

        public async Task<List<PromoCodeDto>> GetListAsync()
        {
            List<PromoCodeDto>? result = null;
            var allcodewithDetails = await _promoCodeRepository.GetListAsync();
            //var list = allsupervisorwithDetails.ToList();
            if (!allcodewithDetails.Any())
            {
                return result;
            }
            result = new List<PromoCodeDto>();
            foreach (var item in allcodewithDetails)
            {

                result.Add(new PromoCodeDto()
                {
                    Id = item.Id,
                    PromoCodeName=item.PromoCodeName,
                    DiscountAmountIn=item.DiscountAmountIn,
                    DiscountAmountInName = item.DiscountAmountIn > 0 ? ((PromoType)item.DiscountAmountIn).ToString() : "n/a",
                    DiscountAmount =item.DiscountAmount,
                    ValidDate=item.ValidDate,
                    MaxCouponAmount=item.MaxCouponAmount,
                    MaxCouponUsedByUser=item.MaxCouponUsedByUser,
                    IsActive = item.IsActive,

                });

            }
            return result;
        }

        public async Task<PromoCodeDto> UpdateAsync(PromoCodeInputDto input)
        {
            var updateItem = ObjectMapper.Map<PromoCodeInputDto, PromoCode>(input);

            var item = await _promoCodeRepository.UpdateAsync(updateItem);
            await _unitOfWorkManager.Current.SaveChangesAsync();
            return ObjectMapper.Map<PromoCode, PromoCodeDto>(item);
        }
    }
}
