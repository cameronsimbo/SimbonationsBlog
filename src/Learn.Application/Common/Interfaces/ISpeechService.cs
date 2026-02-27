namespace Learn.Application.Common.Interfaces;

public interface ISpeechService
{
    Task<string> TranscribeAudioAsync(Stream audioStream, string language, CancellationToken cancellationToken);
    Task<Stream> SynthesizeSpeechAsync(string text, string language, CancellationToken cancellationToken);
}
