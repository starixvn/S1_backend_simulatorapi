using System.Reflection;
using SabaSimulator.API.Helper;

namespace SabaSimulator.API.DependencyInjection;

public static class ServiceCollecitonExtensions
{
	public static IServiceCollection AddSabaPlusApiCore(
	this IServiceCollection services)
	=> services.AddSingleton<IMemoryDb, MemoryDb>();
}