/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.IO;
using MapleLib.WzLib.WzProperties;
using NAudio.Wave;

namespace HaSharedLibrary {
	public class WzMp3Streamer {
		private readonly Stream byteStream;

		private WaveStream waveStream;

		private readonly bool isMP3File = true;

		private readonly WaveOut wavePlayer;
		private readonly WzSoundProperty sound;
		private bool repeat;

		private bool playbackSuccessfully = true;

		public WzMp3Streamer(WzSoundProperty sound, bool repeat) {
			this.sound = sound;
			this.repeat = repeat;
			byteStream = new MemoryStream(sound.GetBytes(false));

			isMP3File = !sound.Name.EndsWith("wav"); // mp3 file does not end with any extension

			wavePlayer = new WaveOut(WaveCallbackInfo.FunctionCallback());
			try {
				if (isMP3File) {
					waveStream = new Mp3FileReader(byteStream);
				} else {
					waveStream = new WaveFileReader(byteStream);
				}
				wavePlayer.Init(waveStream);
			} catch (Exception) {
				playbackSuccessfully = false;
				//InvalidDataException
				// Message = "Not a WAVE file - no RIFF header"
			}

			Volume = 0.5f; // default volume
			wavePlayer.PlaybackStopped += wavePlayer_PlaybackStopped;
		}

		private void wavePlayer_PlaybackStopped(object sender, StoppedEventArgs e) {
			if (!repeat || disposed) {
				return;
			}
			waveStream.Seek(0, SeekOrigin.Begin);

			wavePlayer.Pause();
			wavePlayer.Play();
		}

		private bool disposed;

		public bool Disposed => disposed;

		public void Dispose() {
			if (!playbackSuccessfully) {
				return;
			}

			disposed = true;
			wavePlayer.Dispose();
			if (waveStream != null) {
				waveStream.Dispose();
				waveStream = null;
			}

			byteStream.Dispose();
		}

		public void Play() {
			if (!playbackSuccessfully) {
				return;
			}

			wavePlayer.Play();
		}

		public void Pause() {
			if (!playbackSuccessfully) {
				return;
			}

			wavePlayer.Pause();
		}

		public void Stop() {
			if (!playbackSuccessfully) return;
			wavePlayer.Stop();
		}

		public bool Repeat {
			get => repeat;
			set => repeat = value;
		}

		public int Length => sound.Length / 1000;

		public float Volume {
			get => wavePlayer.Volume;
			set {
				if (value >= 0 && value <= 1.0) wavePlayer.Volume = value;
			}
		}

		public int Position {
			get {
				if (waveStream != null) {
					return (int) (waveStream.Position / waveStream.WaveFormat.AverageBytesPerSecond);
				}

				return 0;
			}
			set => waveStream?.Seek(value * waveStream.WaveFormat.AverageBytesPerSecond, SeekOrigin.Begin);
		}
	}
}