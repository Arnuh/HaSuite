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

		private Mp3FileReader mpegStream;
		private WaveFileReader waveFileStream;

		private readonly bool bIsMP3File = true;

		private readonly WaveOut wavePlayer;
		private readonly WzBinaryProperty sound;
		private bool repeat;

		private bool playbackSuccessfully = true;

		public WzMp3Streamer(WzBinaryProperty sound, bool repeat) {
			this.repeat = repeat;
			this.sound = sound;
			byteStream = new MemoryStream(sound.GetBytes(false));

			bIsMP3File = !sound.Name.EndsWith("wav"); // mp3 file does not end with any extension

			wavePlayer = new WaveOut(WaveCallbackInfo.FunctionCallback());
			try {
				if (bIsMP3File) {
					mpegStream = new Mp3FileReader(byteStream);
					wavePlayer.Init(mpegStream);
				} else {
					waveFileStream = new WaveFileReader(byteStream);
					wavePlayer.Init(waveFileStream);
				}
			} catch (Exception) {
				playbackSuccessfully = false;
				//InvalidDataException
				// Message = "Not a WAVE file - no RIFF header"
			}

			Volume = 0.5f; // default volume
			wavePlayer.PlaybackStopped += wavePlayer_PlaybackStopped;
		}

		private void wavePlayer_PlaybackStopped(object sender, StoppedEventArgs e) {
			if (repeat && !disposed) {
				if (mpegStream != null) {
					mpegStream.Seek(0, SeekOrigin.Begin);
				} else {
					waveFileStream.Seek(0, SeekOrigin.Begin);
				}

				wavePlayer.Pause();
				wavePlayer.Play();
			}
		}

		private bool disposed;

		public bool Disposed => disposed;

		public void Dispose() {
			if (!playbackSuccessfully) {
				return;
			}

			disposed = true;
			wavePlayer.Dispose();
			if (mpegStream != null) {
				mpegStream.Dispose();
				mpegStream = null;
			}

			if (waveFileStream != null) {
				waveFileStream.Dispose();
				waveFileStream = null;
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
				if (mpegStream != null) {
					return (int) (mpegStream.Position / mpegStream.WaveFormat.AverageBytesPerSecond);
				}

				if (waveFileStream != null) {
					return (int) (waveFileStream.Position / waveFileStream.WaveFormat.AverageBytesPerSecond);
				}

				return 0;
			}
			set {
				if (mpegStream != null) {
					mpegStream.Seek(value * mpegStream.WaveFormat.AverageBytesPerSecond, SeekOrigin.Begin);
				} else if (waveFileStream != null) {
					waveFileStream.Seek(value * waveFileStream.WaveFormat.AverageBytesPerSecond, SeekOrigin.Begin);
				}
			}
		}
	}
}