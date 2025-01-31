﻿using System;
using System.IO;

namespace GigE_Cam_Simulator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var path = (args.Length > 0) ? args[0] : ".\\data\\" ;
            var cameraXml = Path.Combine(path, "camera.xml");
            var memoryXml = Path.Combine(path, "memory.xml");

            var preSetMemory = new RegisterConfig(memoryXml);

            var server = new Server(cameraXml, preSetMemory);


            server.OnRegisterChanged(RegisterTypes.Stream_Channel_Packet_Size_0, (mem) =>
            {
                // mem.WriteIntBE(0x128, 17301505); // PixelFormatRegister 
                // mem.WriteIntBE(0x104, 500); // HeightRegister 
               
                //if (mem.ReadIntBE(RegisterTypes.Stream_Channel_Packet_Size_0) != 2080)
                //{
                //    mem.WriteIntBE(RegisterTypes.Stream_Channel_Packet_Size_0, 2080);
                // }
            });

            // on TriggerSoftware
            server.OnRegisterChanged(0x30c, (mem) =>
            {
                if (mem.ReadIntBE(0x124) == 1)
                {
                    Console.WriteLine("--- StartAcquisition");
                    server.StartAcquisition(100);
                }
                else
                {
                    Console.WriteLine("--- StopAcquisition");
                    server.StopAcquisition();
                }
            });

            var imageData = new ImageData[13];
            for (int i = 0; i < 5; i++)
            {
                imageData[i] = ImageData.FormFile(Path.Combine(path, "left" + i.ToString().PadLeft(2, '0') + ".jpg"));
            }

            var imageIndex = 0;
            server.OnAcquiesceImage(() =>
            {
                imageIndex++;
                return imageData[imageIndex % 13];
            });

            server.Run();
            Console.ReadLine();
        }
    }
}