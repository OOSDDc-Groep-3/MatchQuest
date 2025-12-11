using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Controls;
using MatchQuest.Core.Models;

namespace MatchQuest.App.ViewModels
{
    public class GameViewModel : ObservableObject
    {
        private readonly Game _game;

        public GameViewModel(Game game)
        {
            _game = game;
        }

        public int Id => _game.Id;
        public string Name => _game.Name;

        // ImageSource property voor binding in XAML
        public ImageSource ImageSource
        {
            get
            {
                if (string.IsNullOrEmpty(_game.Image))
                    return "placeholder_game.png"; // fallback image

                try
                {
                    byte[] bytes = Convert.FromBase64String(_game.Image);
                    return ImageSource.FromStream(() => new MemoryStream(bytes));
                }
                catch
                {
                    return "placeholder_game.png";
                }
            }
        }
    }
}