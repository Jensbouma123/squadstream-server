using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http;
using Moq;
using NUnit.Framework;
using Web_API_new.Data;
using Web_API_new.DTO.Google;

namespace ServerTests
{
    public class GoogleTests
    {
        private DataContext _dataContext;
        private IHttpClientFactory _httpClientFactory;
        private IConfiguration _configuration;

        [SetUp]
        public void Setup()
        {
            
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("appsettings.json");
            _configuration = configurationBuilder.Build();
            
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dataContext = new DataContext(options);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient());

            _httpClientFactory = mockHttpClientFactory.Object;
        }

        [Test]
        public async Task ValidateRecaptcha()
        {
            // Arrange
            var recaptchaRequest = new CaptchaRequestDTO()
            {
                Token = "03AFcWeA64kF3TmCNf__dPdd4OQ3SjwkVIWUZlLyJyB8fu_HgiqCXbwcFiXrFBiHRI6nqQwn3FuHkaTkOgfmApAdxwO7qFzUYxgeAil_QDrOGkoBkEtpedgSxVzaNWXV0pRIeShynpfTe5BQ1O79auJYRXIYQ-rbDH6N8Z-8UfSeIsM9dLYoWoXQldwj9cRqvdGFSkXGHDlvAjXT1sBzYjeXtL-2v8bMPaBQ1cGjJXqItSn-7q3tTnpLsev4INJO5CR5dGEqlr9NzVFI2du3Treo9rP1ySVAHI8L-HGGDBLbQNNufpny44ycjfUeu_tvJL9WKFK3jaxhu3ix9EdFB8Cus2RrCvR9PdqNgNLQQh1YLf_OhsXRaaYUL07SoCqmc5FzQJ1hTaad8SCYeePKhMeB626v1ffvdQQSmsVncZylRMkbCjnOZ3hXhvTU9JsMMaj-xn4XFDP7FdO9eBGRiepp5PTsjz5y7E5FthaSLJnaIWlRXDLrs-114HzcqdBB_VdjQ1bsskWF4tigTJBl2kn3DJp4M2fizenwqwjYDsYUiZXIx43y1WHEWYKJEZxPpX8ZBu-894b5UWIrT0-6isBkAvLqHJppncOTW97-sn7rpxMUSEDqXGqpClWoIF7TbmiuD-yANoXM_jNMgHn-165yMpMiBScet8oG5J8NbHDKaII8GVhl6KpUfBqOSlWUN_DNecfRN3tU5Ncdq9QhF7Vr1mzQ3NJgthhG8LoIEU8s6FpH7_Xh6ykaK7C9UK4gAkm2mfLUxfJ_DA1s8iqyheWPc5UY15FMKcMV3vGfOFnK53W_QCfkT7F38T5qOkOoXI1kqhsSHEGqmjZMsA7HCKuloPwTkH5Gc0MNCakfJh4DRxlstDoEomDSubKdgbUH7eSs-Bc-4Cula3nZ67NmeMRi0a-Yy606q9nY0HXmWjXTLM1BE5YThzmGAc_kqDxOhDW24DnA1b8tERuXlvKkn6LQbmV6rjcQmmI4k0Cp893eqUKBlYyC4HMuQoowwmrOygsiSbLJ8Ni5da9p29JAVLbQkSRyXQpVa3k8Se0V8gUJqIXgBi3RRWJBVlJicd00Iu0wAuYKCH0H7htB0RICjtm3-kOox_KC-XlHUx17GvCcbDYkvYFxjC3-ksQaUmhzuz8hunfwkH-UPGZNg1f5AFSBo89ykxKm4wORGyEV8MpapDSV1VMyGStHwJNiD1yfb7L3a0UwWf2Z0_XrXB4tvNcPaknaKE9OH7-HRkFY7R16V0VoIA9knSWzjE1K-Sof2Y7C7oRmO4ne1RG5vvXKLloAaedn3bEUFqH6dmCnV1nLtvwKwhx4TZE-wZa5-w_HzRhY9koafoH8DbupbNzVlpsHjz6tuo6LcNRNPB9kS21iGZMDaEUYnoYrIpPZ-JuhiB3Zw2GaqXWkgB3EolyzJwiMD8SHegbUTe9lfiKpwBPMV67sAH8pEC1X5vjM_lPKCCfT2zlkFcu-YqtD3pdrcKOq7hwk55O_PXXBEEC8P46W3u1UC8bMM9lPNZPd6vkDPn7TCujVz2l5EtFHqdhncDz3o6gb5bx4G6S68hrxxxRG5JZFe2w0lCdaEcWszks_b_jNzfD-ZYVsy31r3IT1MGNyaL1NhnPNSWGJGWoMVoA7dunjSyw-rGIz4a5J-JPHlr4ah-5st0tjoVWmEXv709nS9RF-V8UpTMfMxRuh6HSntADbWYUAP2aM0EsMyG7R6hwVy15d4EbsLLvlT_EnJ4-YuoX19V2ahKERFciGOxECLGfK5TFeDicv94zLAEl7ZwC8oBQEt77goLf5Rlqs_ulNC0oUHkVeg3flxfPFtwKir75XQHznedRefTZ7JH7WeC4KJ9jSvMXURJrIyh_bCRlKbxsIjxSRZ977a4XLlpFsVPm5gZMXKg3xSDejUMYVybtMPNPtVyHM22wiq5WJ4j_48KaerxOOKlbReSxga0vd9XkknWDgb0pPlUX_GmQVNr7xmBMzU83RN1"
            };

            var client = _httpClientFactory.CreateClient();
            
            // Act
            var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify",
                new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("secret", _configuration["RecaptchaSettings:SecretKey"]),
                    new KeyValuePair<string, string>("response", recaptchaRequest.Token),
                }));

            // Assert
            Assert.IsTrue(response.IsSuccessStatusCode);
        }
    }
}