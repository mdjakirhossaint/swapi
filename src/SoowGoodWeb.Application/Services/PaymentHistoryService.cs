using SoowGoodWeb.DtoModels;
using SoowGoodWeb.InputDto;
using SoowGoodWeb.Interfaces;
using SoowGoodWeb.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace SoowGoodWeb.Services
{
    public class PaymentHistoryService : SoowGoodWebAppService, IPaymentHistoryService
    {
        private readonly IRepository<PaymentHistory> _paymentHistoryRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        public PaymentHistoryService(IRepository<PaymentHistory> paymentHistoryRepository, IUnitOfWorkManager unitOfWorkManager)
        {
            _paymentHistoryRepository = paymentHistoryRepository;

            _unitOfWorkManager = unitOfWorkManager;
        }
        public async Task<PaymentHistoryDto> CreateAsync(PaymentHistoryInputDto input)
        {
            var newEntity = ObjectMapper.Map<PaymentHistoryInputDto, PaymentHistory>(input);

            var doctorProfile = await _paymentHistoryRepository.InsertAsync(newEntity);

            //await _unitOfWorkManager.Current.SaveChangesAsync();

            return ObjectMapper.Map<PaymentHistory, PaymentHistoryDto>(doctorProfile);
        }

        public async Task<PaymentHistoryDto> GetAsync(int id)
        {
            var item = await _paymentHistoryRepository.GetAsync(x => x.Id == id);

            return ObjectMapper.Map<PaymentHistory, PaymentHistoryDto>(item);
        }
        public async Task<List<PaymentHistoryDto>> GetListAsync()
        {
            var specialization = await _paymentHistoryRepository.GetListAsync();
            return ObjectMapper.Map<List<PaymentHistory>, List<PaymentHistoryDto>>(specialization);
        }
        public async Task<PaymentHistoryDto> UpdateAsync(PaymentHistoryInputDto input)
        {
            var updateItem = ObjectMapper.Map<PaymentHistoryInputDto, PaymentHistory>(input);

            var item = await _paymentHistoryRepository.UpdateAsync(updateItem);

            return ObjectMapper.Map<PaymentHistory, PaymentHistoryDto>(item);
        }
        public async Task<PaymentHistoryDto> GetByTranIdAsync(string tranId)
        {
            var item = await _paymentHistoryRepository.GetAsync(x => x.tran_id == tranId);

            return ObjectMapper.Map<PaymentHistory, PaymentHistoryDto>(item);
        }
        public async Task<string> GetByAppointmentCodeAsync(string appCode)
        {
            var item = await _paymentHistoryRepository.GetAsync(x => x.application_code == appCode);
            var tranId = item.tran_id;
            return (!string.IsNullOrEmpty(tranId) ? tranId : "");// ObjectMapper.Map<PaymentHistory, PaymentHistoryDto>(item);
        }
        public async Task<bool> UpdateHistoryAsync(PaymentHistoryInputDto input)
        {
            try
            {
                var paymentHistory = await _paymentHistoryRepository.GetAsync(p => p.Id == input.Id);

                if (paymentHistory == null)
                {
                    Console.WriteLine($"No PaymentHistory found with ID: {input.Id}");
                    return false;
                }

                paymentHistory.val_id = input?.val_id;
                paymentHistory.amount = input?.amount;
                paymentHistory.card_type = input?.card_type;
                paymentHistory.store_amount = input?.store_amount;
                paymentHistory.card_no = input?.card_no;
                paymentHistory.bank_tran_id = input?.bank_tran_id;
                paymentHistory.status = input?.status;
                paymentHistory.tran_date = input?.tran_date;
                paymentHistory.failedreason = input?.error;
                paymentHistory.error = input?.error;
                paymentHistory.currency = input?.currency;
                paymentHistory.card_issuer = input?.card_issuer;
                paymentHistory.card_brand = input?.card_brand;
                paymentHistory.card_sub_brand = input?.card_sub_brand;
                paymentHistory.card_issuer_country = input?.card_issuer_country;
                paymentHistory.card_issuer_country_code = input?.card_issuer_country_code;
                paymentHistory.currency_type = input?.currency_type;
                paymentHistory.currency_amount = input?.currency_amount;
                paymentHistory.currency_rate = input?.currency_rate;
                paymentHistory.base_fair = input?.base_fair;
                paymentHistory.value_a = input?.value_a;
                paymentHistory.value_b = input?.value_b;
                paymentHistory.value_c = input?.value_c;
                paymentHistory.value_d = input?.value_d;
                paymentHistory.subscription_id = input?.subscription_id;
                paymentHistory.risk_level = input?.risk_level;
                paymentHistory.risk_title = input?.risk_title;

                await _paymentHistoryRepository.UpdateAsync(paymentHistory);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while updating payment history: " + ex.Message);
                return false;
            }
        }


    }
}
