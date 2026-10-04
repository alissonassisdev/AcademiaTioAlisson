// Alisson Assis
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AcademiaTioAlisson.Presentation.AppMaui.Message;

public sealed class TemaPreferencesUpdatedMessage(string value) : ValueChangedMessage<string>(value)
{
}