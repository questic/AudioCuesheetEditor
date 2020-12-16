﻿//This file is part of AudioCuesheetEditor.

//AudioCuesheetEditor is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//AudioCuesheetEditor is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with Foobar.  If not, see
//<http: //www.gnu.org/licenses />.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AudioCuesheetEditor.Model.IO;
using System;
using System.Collections.Generic;
using System.Text;
using AudioCuesheetEditor.Model.AudioCuesheet;
using System.IO;
using System.Linq;
using AudioCuesheetEditor.Controller;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using AudioCuesheetEditor.Shared.ResourceFiles;
using AudioCuesheetEditorTests.Utility;

namespace AudioCuesheetEditor.Model.IO.Tests
{
    [TestClass()]
    public class CuesheetFileTests
    {
        [TestMethod()]
        public void GenerateCuesheetFileTest()
        {
            var testHelper = new TestHelper();
            Cuesheet cuesheet = testHelper.CuesheetController.Cuesheet;
            cuesheet.Artist = "Demo Artist";
            cuesheet.Title = "Demo Title";
            cuesheet.AudioFile = new AudioFile("Testfile.mp3");
            var begin = TimeSpan.Zero;
            for (int i = 1; i < 25; i++)
            {
                var track = testHelper.CuesheetController.NewTrack();
                track.Artist = String.Format("Demo Track Artist {0}", i);
                track.Title = String.Format("Demo Track Title {0}", i);
                track.Begin = begin;
                begin = begin.Add(new TimeSpan(0, i, i));
                track.End = begin;
                cuesheet.AddTrack(track);
            }
            var cuesheetFile = new CuesheetFile(cuesheet);
            var generatedFile = cuesheetFile.GenerateCuesheetFile();
            Assert.IsNotNull(generatedFile);
            var fileName = Path.GetTempFileName();
            File.WriteAllBytes(fileName, generatedFile);
            var fileContent = File.ReadAllLines(fileName);
            Assert.AreEqual(fileContent[0], String.Format("{0} \"{1}\"", CuesheetFile.CuesheetTitle, cuesheet.Title));
            Assert.AreEqual(fileContent[1], String.Format("{0} \"{1}\"", CuesheetFile.CuesheetArtist, cuesheet.Artist));
            Assert.AreEqual(fileContent[2], String.Format("{0} \"{1}\" {2}", CuesheetFile.CuesheetFileName, cuesheet.AudioFile.FileName, cuesheet.AudioFile.AudioFileType));
            var position = 1;
            for (int i = 3; i < fileContent.Length; i += 4)
            {
                var track = cuesheet.Tracks.Single(x => x.Position == position);
                position++;
                Assert.AreEqual(fileContent[i], String.Format("{0}{1} {2:00} {3}", CuesheetFile.Tab, CuesheetFile.CuesheetTrack, track.Position, CuesheetFile.CuesheetTrackAudio));
                Assert.AreEqual(fileContent[i + 1], String.Format("{0}{1}{2} \"{3}\"", CuesheetFile.Tab, CuesheetFile.Tab, CuesheetFile.TrackTitle, track.Title));
                Assert.AreEqual(fileContent[i + 2], String.Format("{0}{1}{2} \"{3}\"", CuesheetFile.Tab, CuesheetFile.Tab, CuesheetFile.TrackArtist, track.Artist));
                Assert.AreEqual(fileContent[i + 3], String.Format("{0}{1}{2} {3:00}:{4:00}:{5:00}", CuesheetFile.Tab, CuesheetFile.Tab, CuesheetFile.TrackIndex01, Math.Floor(track.Begin.Value.TotalMinutes), track.Begin.Value.Seconds, track.Begin.Value.Milliseconds / 75));
            }
            File.Delete(fileName);
        }
    }
}