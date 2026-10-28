using System;

public interface ILLMService
{
    void AskAI(string promptText, Action<string, bool> onResponseReceived);
}