﻿using MareSynchronos.API.Routes;
using MareSynchronosShared.Services;
using MareSynchronosShared.Utils;
using MareSynchronosShared.Utils.Configuration;
using System.Net.Http.Headers;

namespace MareSynchronosStaticFilesServer.Services;

public class ShardClientReadyMessageService : IClientReadyMessageService
{
    private readonly ILogger<ShardClientReadyMessageService> _logger;
    private readonly ServerTokenGenerator _tokenGenerator;
    private readonly IConfigurationService<StaticFilesServerConfiguration> _configurationService;
    private readonly HttpClient _httpClient;

    public ShardClientReadyMessageService(ILogger<ShardClientReadyMessageService> logger, ServerTokenGenerator tokenGenerator, IConfigurationService<StaticFilesServerConfiguration> configurationService)
    {
        _logger = logger;
        _tokenGenerator = tokenGenerator;
        _configurationService = configurationService;
        _httpClient = new();
        _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("MareSynchronosServer", "1.0.0.0"));
    }

    public void SendDownloadReady(string uid, Guid requestId)
    {
        _ = Task.Run(async () =>
        {
            var mainUrlConfigKey = _configurationService.GetValue<bool>(nameof(StaticFilesServerConfiguration.NotifyMainServerDirectly))
                ? nameof(StaticFilesServerConfiguration.MainServerAddress)
                : nameof(StaticFilesServerConfiguration.MainFileServerAddress);
            var mainUrl = _configurationService.GetValue<Uri>(mainUrlConfigKey);
            var path = MareFiles.MainSendReadyFullPath(mainUrl, uid, requestId);
            using HttpRequestMessage msg = new()
            {
                RequestUri = path
            };
            msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenGenerator.Token);

            _logger.LogDebug("Sending Client Ready for {uid}:{requestId} to {path}", uid, requestId, path);
            try
            {
                using var result = await _httpClient.SendAsync(msg).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failure to send for {uid}:{requestId}", uid, requestId);
            }
        });
    }
}
