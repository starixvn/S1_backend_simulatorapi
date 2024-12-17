using Newtonsoft.Json;
using SabaSimulator.API.Helper;
using SabaSimulator.API.Models.SabaPlus;

namespace SabaSimulator.API.ApiClient.Handler;

public class SabaPlusHandler : DelegatingHandler
{
	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		if (request.Content != null && request.Method == HttpMethod.Post)
		{
			var bodyContent = await request.Content.ReadAsStringAsync(cancellationToken);

			var query = JsonConvert.DeserializeObject<GameProviderRequest<object>>(bodyContent);

			var operatorSign = MD5Helper.ToMD5(bodyContent + query?.OperatorKey);

			request.Headers.Add("operator-sign", operatorSign);
		}

		return await base.SendAsync(request, cancellationToken);
	}
}