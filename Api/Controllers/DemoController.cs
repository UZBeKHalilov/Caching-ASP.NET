using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class DemoController : ControllerBase
{
    private readonly DemoService _svc;
    public DemoController(DemoService svc) => _svc = svc;



    [HttpGet("mem/{key}")]
    public async Task<IActionResult> GetFromMemory(string key) =>
        Ok(await _svc.GetMemoryCachedDataAsync(key));




    [HttpGet("redis/{key}")]
    public async Task<IActionResult> GetFromRedis(string key) =>
        Ok(await _svc.GetRedisCachedDataAsync(key));



    [HttpPost("save/{key}")]
    public async Task<IActionResult> Save(string key, [FromBody] string data)
    {
        await _svc.SaveDataWithWriteThroughAsync(key, data);
        return Ok();
    }
}