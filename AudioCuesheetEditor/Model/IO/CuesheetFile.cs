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
using AudioCuesheetEditor.Controller;
using AudioCuesheetEditor.Model.AudioCuesheet;
using AudioCuesheetEditor.Model.IO.Audio;
using AudioCuesheetEditor.Model.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.IO
{
    public class CuesheetFile
    {
        public const String MimeType = "text/*";
        public const String FileExtension = ".cue";

        public static readonly String DefaultFileName = "Cuesheet.cue";

        public static readonly String CuesheetArtist = "PERFORMER";
        public static readonly String CuesheetTitle = "TITLE";
        public static readonly String CuesheetFileName = "FILE";
        public static readonly String CuesheetTrack = "TRACK";
        public static readonly String CuesheetTrackAudio = "AUDIO";
        public static readonly String TrackArtist = "PERFORMER";
        public static readonly String TrackTitle = "TITLE";
        public static readonly String TrackIndex01 = "INDEX 01";
        public static readonly String TrackFlags = "FLAGS";
        public static readonly String TrackPreGap = "PREGAP";
        public static readonly String TrackPostGap = "POSTGAP";
        public static readonly String Tab = "\t";
        public static readonly String CuesheetCDTextfile = "CDTEXTFILE";
        public static readonly String CuesheetCatalogueNumber = "CATALOG";

        public static Cuesheet ImportCuesheet(MemoryStream fileContent, ApplicationOptions applicationOptions)
        {
            if (fileContent == null)
            {
                throw new ArgumentNullException(nameof(fileContent));
            }
            if (applicationOptions == null)
            {
                throw new ArgumentNullException(nameof(applicationOptions));
            }
            var cuesheet = new Cuesheet();
            fileContent.Position = 0;
            using var reader = new StreamReader(fileContent);
            var regexCuesheetArtist = new Regex("^" + CuesheetArtist);
            var regexCuesheetTitle = new Regex("^" + CuesheetTitle);
            var regexCuesheetFile = new Regex("^" + CuesheetFileName);
            var regexTrackBegin = new Regex(CuesheetTrack + " [0-9]{1,} " + CuesheetTrackAudio);
            var regexTrackArtist = new Regex("(?<!^)" + TrackArtist);
            var regexTrackTitle = new Regex("(?<!^)" + TrackTitle);
            var regexTrackIndex = new Regex("(?<!^)" + TrackIndex01);
            var regexTrackFlags = new Regex("(?<!^)" + TrackFlags);
            var regexTrackPreGap = new Regex("(?<!^)" + TrackPreGap);
            var regexTrackPostGap = new Regex("(?<!^)" + TrackPostGap);
            var regexCDTextfile = new Regex(String.Format("^{0}", CuesheetCDTextfile));
            var regexCatalogueNumber = new Regex(String.Format("^{0} ", CuesheetCatalogueNumber));
            //TODO: Match frames also and import
            var regExTimespanValue = new Regex("[0-9]{2,}:[0-9]{2,}");
            Track track = null;
            while (reader.EndOfStream == false)
            {
                var line = reader.ReadLine();
                if (regexCuesheetArtist.IsMatch(line) == true)
                {
                    var artist = line.Substring(line.IndexOf("\"") + 1, line.LastIndexOf("\"") - (line.IndexOf("\"") + 1));
                    cuesheet.Artist = artist;
                }
                if (regexCuesheetTitle.IsMatch(line) == true)
                {
                    var title = line.Substring(line.IndexOf("\"") + 1, line.LastIndexOf("\"") - (line.IndexOf("\"") + 1));
                    cuesheet.Title = title;
                }
                if (regexCuesheetFile.IsMatch(line) == true)
                {
                    var audioFile = line.Substring(line.IndexOf("\"") + 1, line.LastIndexOf("\"") - (line.IndexOf("\"") + 1));
                    cuesheet.AudioFile = new AudioFile(audioFile);
                }
                if (regexCDTextfile.IsMatch(line) == true)
                {
                    var cdTextfile = line.Substring(line.IndexOf("\"") + 1, line.LastIndexOf("\"") - (line.IndexOf("\"") + 1));
                    cuesheet.CDTextfile = new CDTextfile(cdTextfile);
                }
                if (regexCatalogueNumber.IsMatch(line) == true)
                {
                    var catalogueNumber = line.Substring(regexCatalogueNumber.Match(line).Length);
                    cuesheet.CatalogueNumber.Value = catalogueNumber;
                }
                if (regexTrackBegin.IsMatch(line) == true)
                {
                    track = new Track();
                }
                if (regexTrackArtist.IsMatch(line) == true)
                {
                    var artist = line.Substring(line.IndexOf("\"") + 1, line.LastIndexOf("\"") - (line.IndexOf("\"") + 1));
                    track.Artist = artist;
                }
                if (regexTrackTitle.IsMatch(line) == true)
                {
                    var title = line.Substring(line.IndexOf("\"") + 1, line.LastIndexOf("\"") - (line.IndexOf("\"") + 1));
                    track.Title = title;
                }
                if (regexTrackFlags.IsMatch(line) == true)
                {
                    var match = regexTrackFlags.Match(line);
                    var flags = line.Substring(match.Index + match.Length + 1);
                    var flagList = Flag.AvailableFlags.Where(x => flags.Contains(x.CuesheetLabel));
                    track.SetFlags(flagList);
                }
                if (regexTrackPreGap.IsMatch(line) == true)
                {
                    var match = regExTimespanValue.Match(line);
                    if (match.Success == true)
                    {
                        var minutes = int.Parse(match.Value.Substring(0, match.Value.IndexOf(":")));
                        var seconds = int.Parse(match.Value.Substring(match.Value.IndexOf(":") + 1));
                        track.PreGap = new TimeSpan(0, minutes, seconds);
                    }
                }
                if (regexTrackIndex.IsMatch(line) == true)
                {
                    var match = regExTimespanValue.Match(line);
                    if (match.Success == true) 
                    {
                        var minutes = int.Parse(match.Value.Substring(0, match.Value.IndexOf(":")));
                        var seconds = int.Parse(match.Value.Substring(match.Value.IndexOf(":") + 1));
                        track.Begin = new TimeSpan(0, minutes, seconds);
                    }
                    cuesheet.AddTrack(track, applicationOptions);
                }
                if (regexTrackPostGap.IsMatch(line) == true)
                {
                    var match = regExTimespanValue.Match(line);
                    if (match.Success == true)
                    {
                        var minutes = int.Parse(match.Value.Substring(0, match.Value.IndexOf(":")));
                        var seconds = int.Parse(match.Value.Substring(match.Value.IndexOf(":") + 1));
                        track.PostGap = new TimeSpan(0, minutes, seconds);
                    }
                }
            }
            return cuesheet;
        }

        public CuesheetFile(Cuesheet cuesheet)
        {
            if (cuesheet == null)
            {
                throw new ArgumentNullException(nameof(cuesheet));
            }
            Cuesheet = cuesheet;
        }

        public Cuesheet Cuesheet { get; private set; }

        public byte[] GenerateCuesheetFile()
        {
            if (IsExportable == true)
            {
                var builder = new StringBuilder();
                if ((Cuesheet.CatalogueNumber != null) && (Cuesheet.CatalogueNumber.IsValid == true))
                {
                    builder.AppendLine(String.Format("{0} {1}", CuesheetCatalogueNumber, Cuesheet.CatalogueNumber.Value));
                }
                if (Cuesheet.CDTextfile != null)
                {
                    builder.AppendLine(String.Format("{0} \"{1}\"", CuesheetCDTextfile, Cuesheet.CDTextfile.FileName));
                }
                builder.AppendLine(String.Format("{0} \"{1}\"", CuesheetTitle, Cuesheet.Title));
                builder.AppendLine(String.Format("{0} \"{1}\"", CuesheetArtist, Cuesheet.Artist));
                builder.AppendLine(String.Format("{0} \"{1}\" {2}", CuesheetFileName, Cuesheet.AudioFile.FileName, Cuesheet.AudioFile.AudioFileType));
                foreach (var track in Cuesheet.Tracks)
                {
                    builder.AppendLine(String.Format("{0}{1} {2:00} {3}", Tab, CuesheetTrack, track.Position, CuesheetTrackAudio));
                    builder.AppendLine(String.Format("{0}{1}{2} \"{3}\"", Tab, Tab, TrackTitle, track.Title));
                    builder.AppendLine(String.Format("{0}{1}{2} \"{3}\"", Tab, Tab, TrackArtist, track.Artist));
                    if (track.Flags.Count > 0)
                    {
                        builder.AppendLine(String.Format("{0}{1}{2} {3}", Tab, Tab, TrackFlags, String.Join(" ", track.Flags.Select(x => x.CuesheetLabel))));
                    }
                    if (track.PreGap.HasValue)
                    {
                        builder.AppendLine(String.Format("{0}{1}{2} {3:00}:{4:00}:{5:00}", Tab, Tab, TrackPreGap, Math.Floor(track.PreGap.Value.TotalMinutes), track.PreGap.Value.Seconds, track.PreGap.Value.Milliseconds / 75));
                    }
                    builder.AppendLine(String.Format("{0}{1}{2} {3:00}:{4:00}:{5:00}", Tab, Tab, TrackIndex01, Math.Floor(track.Begin.Value.TotalMinutes), track.Begin.Value.Seconds, track.Begin.Value.Milliseconds / 75));
                    if (track.PostGap.HasValue)
                    {
                        builder.AppendLine(String.Format("{0}{1}{2} {3:00}:{4:00}:{5:00}", Tab, Tab, TrackPostGap, Math.Floor(track.PostGap.Value.TotalMinutes), track.PostGap.Value.Seconds, track.PostGap.Value.Milliseconds / 75));
                    }
                }
                return Encoding.UTF8.GetBytes(builder.ToString());
            }
            else
            {
                return null;
            }
        }

        public Boolean IsExportable
        {
            get
            {
                if (Cuesheet.GetValidationErrorsFiltered(validationErrorFilterType: Entity.ValidationErrorFilterType.ErrorOnly).Count > 0)
                {
                    return false;
                }
                if (Cuesheet.Tracks.Any(x => x.GetValidationErrorsFiltered(validationErrorFilterType: Entity.ValidationErrorFilterType.ErrorOnly).Count > 0) == true)
                { 
                    return false;
                }
                return true;
            }
        }
    }
}
