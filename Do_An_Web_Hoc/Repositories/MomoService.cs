using Do_An_Web_Hoc.Models;
using Do_An_Web_Hoc.Repositories.Interfaces;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Do_An_Web_Hoc.Repositories
{
    public class MomoService : IMomoService
    {
        private readonly IOptions<MomoOptionModel> _options;
        public MomoService(IOptions<MomoOptionModel> options)
        {
            _options = options;
        }

        public async Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfoModel model)
        {
            // Gắn thông tin đơn hàng
            model.OrderInfo = $"Khách hàng: {model.FullName}. Nội dung: {model.OrderInfo}";
            model.OrderId = Guid.NewGuid().ToString(); // Tránh trùng orderId

            // Tạo chuỗi rawData
            var rawData =
                $"partnerCode={_options.Value.PartnerCode}" +
                $"&accessKey={_options.Value.AccessKey}" +
                $"&requestId={model.OrderId}" +
                $"&amount={model.Amount}" +
                $"&orderId={model.OrderId}" +
                $"&orderInfo={model.OrderInfo}" +
                $"&returnUrl={_options.Value.ReturnUrl}" +
                $"&notifyUrl={_options.Value.NotifyUrl}" +
                $"&extraData=";

            // Ký HMAC SHA256
            var signature = ComputeHmacSha256(rawData, _options.Value.SecretKey);

            // Dữ liệu gửi đi
            var requestData = new
            {
                partnerCode = _options.Value.PartnerCode,
                accessKey = _options.Value.AccessKey,
                requestId = model.OrderId,
                amount = model.Amount.ToString(),
                orderId = model.OrderId,
                orderInfo = model.OrderInfo,
                returnUrl = _options.Value.ReturnUrl,
                notifyUrl = _options.Value.NotifyUrl,
                extraData = "",
                requestType = _options.Value.RequestType,
                signature = signature
            };

            var client = new HttpClient();
            var response = await client.PostAsJsonAsync(_options.Value.MomoApiUrl, requestData);
            var json = await response.Content.ReadAsStringAsync();

            Console.WriteLine("[DEBUG] JSON RESPONSE MoMo:");
            Console.WriteLine(json);

            // ⚠️ Kiểm tra nếu không phải JSON hợp lệ
            if (!response.IsSuccessStatusCode || json.StartsWith("<"))
            {
                Console.WriteLine("[ERROR] Phản hồi từ MoMo không hợp lệ hoặc bị lỗi 403.");
                return null;
            }

            try
            {
                var momoResponse = JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(json);
                return momoResponse;
            }
            catch (JsonReaderException ex)
            {
                Console.WriteLine("[ERROR] Lỗi khi deserialize JSON từ MoMo: " + ex.Message);
                return null;
            }
        }


        public MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection)
        {
            return new MomoExecuteResponseModel
            {
                Amount = collection["amount"],
                OrderId = collection["orderId"],
                OrderInfo = collection["orderInfo"]
            };
        }

        private string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);
            using var hmac = new HMACSHA256(keyBytes);
            return BitConverter.ToString(hmac.ComputeHash(messageBytes)).Replace("-", "").ToLower();
        }
    }
}
