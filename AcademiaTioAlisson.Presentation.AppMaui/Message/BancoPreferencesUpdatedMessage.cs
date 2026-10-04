// Alisson Assis
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AcademiaTioAlisson.Presentation.AppMaui.Message;

public sealed class BancoPreferencesUpdatedMessage(string value) : ValueChangedMessage<string>(value)
{
}