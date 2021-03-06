﻿using DayActive.Engine.App.Models;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using System.Threading;


namespace DayActive.Engine.App.Helpers
{
    public static class DayActiveController
    {
        public static System.Threading.Timer _timer { get; set; }
        public static System.Timers.Timer GameHeartBeatTimer;
        public static DataPayLoad DataPayLoad;
        public static GameDataObject GameDataObject;
        public static async void RegisterGameMetaData(GameDataObject gameDataObject)
        {
            try
            {
                GameDataObject = gameDataObject;
                HttpResponseMessage response = await Connector.Client.PostAsJsonAsync(
                    "game_metadata", gameDataObject.GameMetaData);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                string currentMethodName = ErrorHandling.GetCurrentMethodName();
                ErrorHandling.LogErrorToTxtFile(ex, currentMethodName);
                if (Connector.EstablishConnection(gameDataObject))
                {
                    RegisterGameMetaData(gameDataObject);
                }
            }
        }

        public static async void BindGameEvent(GameDataObject gameDataObject)
        {
            try
            {
                HttpResponseMessage response = await Connector.Client.PostAsJsonAsync(
                    "bind_game_event", gameDataObject.GameEventHandler);
                response.EnsureSuccessStatusCode();
            }

            catch (Exception ex)
            {
                string currentMethodName = ErrorHandling.GetCurrentMethodName();
                ErrorHandling.LogErrorToTxtFile(ex, currentMethodName);
                if (Connector.EstablishConnection(gameDataObject))
                {
                    BindGameEvent(gameDataObject);
                }
            }
        }

        public static void StartMonitoringTimer(GameDataObject gameDataObject)
        {
            try
            {
                _timer = new System.Threading.Timer(
                    _ =>
                    {
                        CalculateTimeLeft();
                    },
                    null,
                    TimeSpan.FromSeconds(8),
                    TimeSpan.FromSeconds(60)
                );

            }
            catch (Exception ex)
            {
                string currentMethodName = ErrorHandling.GetCurrentMethodName();
                ErrorHandling.LogErrorToTxtFile(ex, currentMethodName);
            }

        }

        private static async void CalculateTimeLeft()
        {
            try
            {
                //Calculate time left in the day
                var remainingTimeString = TimeSpan.FromHours(24) - DateTime.Now.TimeOfDay;
                var remainingHourInPercentage = Math.Round(((remainingTimeString.TotalHours / 24) * 100), 1);
                DataPayLoad = new DataPayLoad()
                {
                    Game = "DAYACTIVE",
                    Event = "DISPLAY_TIME",
                    Data = new Data()
                    {
                        Value = remainingHourInPercentage.ToString()
                    }
                };

                HttpResponseMessage response = await Connector.Client.PostAsJsonAsync(
                    "game_event", DataPayLoad);
                response.EnsureSuccessStatusCode();

            }
            catch (Exception ex)
            {
                string currentMethodName = ErrorHandling.GetCurrentMethodName();
                ErrorHandling.LogErrorToTxtFile(ex, currentMethodName);
            }

        }

        public static async void DisplayWelcomeScreenAsync(GameDataObject gameDataObject)
        {
            try
            {
                await Connector.BindWelcomeEventAsync(gameDataObject.WelcomeEventHandler);
                HttpResponseMessage response = await Connector.Client.PostAsJsonAsync(
                    "game_event", gameDataObject.WelcomeDataPayLoad);
            }
            catch (Exception ex)
            {
                string currentMethodName = ErrorHandling.GetCurrentMethodName();
                ErrorHandling.LogErrorToTxtFile(ex, currentMethodName);
                if (Connector.EstablishConnection(gameDataObject))
                {
                    DisplayWelcomeScreenAsync(gameDataObject);
                }
            }
        }

        public static void StartGameHeartBeatTimer()
        {
            try
            {
                GameHeartBeatTimer = new System.Timers.Timer();
                GameHeartBeatTimer.Interval = 10000;
                GameHeartBeatTimer.Elapsed += RefreshScreen;
                GameHeartBeatTimer.Start();
            }
            catch (Exception ex)
            {
                string currentMethodName = ErrorHandling.GetCurrentMethodName();
                ErrorHandling.LogErrorToTxtFile(ex, currentMethodName);
            }
        }

        //Resfresh the screen to keep it alive. 
        public static async void RefreshScreen(object sender, ElapsedEventArgs e)
        {
            try
            {
                GameHeartBeat gameHeartBeat = new GameHeartBeat()
                {
                    Game = "DAYACTIVE"
                };

                HttpResponseMessage keepAliveResponse = await Connector.Client.PostAsJsonAsync(
                    "game_heartbeat", gameHeartBeat);
                keepAliveResponse.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                GameHeartBeatTimer.Stop();
                string currentMethodName = ErrorHandling.GetCurrentMethodName();
                ErrorHandling.LogErrorToTxtFile(ex, currentMethodName);

                if (Connector.EstablishConnection(GameDataObject))
                {
                    StartGameHeartBeatTimer();
                    CalculateTimeLeft();
                }
            }
        }
    }
}
