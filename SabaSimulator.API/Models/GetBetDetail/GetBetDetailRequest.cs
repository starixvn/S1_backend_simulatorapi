using System.Text.Json.Serialization;

namespace SabaSimulator.API.Models.GetBetDetail;

public class GetBetDetailRequest
{
	public string vendor_id { get; set; }

	public long version_key { get; set; }
}