using Learn.Application.Common.Interfaces;

namespace Learn.Infrastructure.Services;

public class SpeechService : ISpeechService
{
    public async Task<string> TranscribeAudioAsync(Stream audioStream, string language, CancellationToken cancellationToken)
    {
        // TODO: Implement Azure Speech Services integration
        await Task.CompletedTask;
        return "Placeholder transcription — Azure Speech Services not yet configured.";
    }

    public async Task<Stream> SynthesizeSpeechAsync(string text, string language, CancellationToken cancellationToken)
    {
        // TODO: Implement Azure Speech Services integration
        await Task.CompletedTask;
        return new MemoryStream();
    }
}
