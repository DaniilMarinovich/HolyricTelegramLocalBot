using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WindowsInput;
using WindowsInput.Native;

namespace TelegramKeySimulator
{
    class Program
    {
        private static ITelegramBotClient botClient;
        private static InputSimulator inputSimulator = new InputSimulator();
        private static int messageId;

        static async Task Main(string[] args)
        {
            botClient = new TelegramBotClient("7336730044:AAEKucHuqd_ZByPRknphtePhYfxMf2tdY6Q");

            using var cts = new CancellationTokenSource();
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken: cts.Token
            );

            var me = await botClient.GetMeAsync();
            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            // Send cancellation request to stop bot
            cts.Cancel();
        }

        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message.Text == "/start")
            {
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Вперед", "forward"),
                        InlineKeyboardButton.WithCallbackData("Назад", "backward")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("+ звук", "volume_up"),
                        InlineKeyboardButton.WithCallbackData("- звук", "volume_down")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("+ звук 10", "volume_up_10"),
                        InlineKeyboardButton.WithCallbackData("- звук 10", "volume_down_10")
                    }
                });

                var message = await botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: "Выберите действие:",
                    replyMarkup: inlineKeyboard,
                    cancellationToken: cancellationToken
                );

                // Store the message ID for later use
                messageId = message.MessageId;
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                var action = update.CallbackQuery.Data;
                await Console.Out.WriteLineAsync($"User:{update.CallbackQuery.From.FirstName} || Query: {action}");

                switch (action)
                {
                    case "forward":
                        inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RIGHT);
                        break;
                    case "backward":
                        inputSimulator.Keyboard.KeyPress(VirtualKeyCode.LEFT);
                        break;
                    case "volume_up":
                        inputSimulator.Keyboard.KeyPress(VirtualKeyCode.VOLUME_UP);
                        break;
                    case "volume_down":
                        inputSimulator.Keyboard.KeyPress(VirtualKeyCode.VOLUME_DOWN);
                        break;
                    case "volume_up_10":
                        for (int i = 0; i < 10; i++)
                        {
                            inputSimulator.Keyboard.KeyPress(VirtualKeyCode.VOLUME_UP);
                        }
                        break;
                    case "volume_down_10":
                        for (int i = 0; i < 10; i++)
                        {
                            inputSimulator.Keyboard.KeyPress(VirtualKeyCode.VOLUME_DOWN);
                        }
                        break;
                }
            }
        }

        private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Error: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}
