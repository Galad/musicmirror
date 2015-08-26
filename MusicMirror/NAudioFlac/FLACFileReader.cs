#region © Copyright 2010 Yuval Naveh. MIT.
/* Copyright (c) 2010, Yuval Naveh

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
#endregion

using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using NAudio.Wave;
using System.Runtime.InteropServices;
using System.Threading;

namespace BigMansStuff.NAudio.FLAC
{
	/// <summary>
	/// Read a wave stream from a flac stream
	/// </summary>
	/// <remarks>
	/// Written By Yuval Naveh, based a .NET/C# Interop wrapper by Stanimir Stoyanov - http://stoyanoff.info/blog/2010/07/26/decoding-flac-audio-files-in-c/
	/// using libFlac - http://flac.sourceforge.net
	/// </remarks>
	public class WaveStreamFromFlacStream : WaveStream
	{
		/// <summary>Creates a new instance of <c>WaveStreamFromFlacStream</c></summary>
		///<param name="flacStream">The stream object containing the raw Flac data</param>
		public WaveStreamFromFlacStream(Stream flacStream)
		{
			if (flacStream == null) throw new ArgumentNullException(nameof(flacStream));
			// Open the flac file for reading through a binary reader
			_stream = flacStream;
			// Create the FLAC decoder
			_decoderContext = LibFLACSharp.FLAC__stream_decoder_new();

			if (_decoderContext == IntPtr.Zero)
				throw new FlacException("The FLAC decoder could not be initialized. There may not be enough memory to allocate a new decoder.");

			// keep instance of each managed delegate because they will be marshalled
			// if we do not keep a reference they will be garbage collected
			_readCallback = ReadCallback;
			_seekCallback = SeekCallback;
			_tellCallback = TellCallback;
			_lengthCallback = LengthCallback;
			_eofCallback = EofCallback;
			_writeCallback = WriteCallback;
			_metadataCallback = MetadataCallback;
			_errorCallback = ErrorCallback;

			// Initialize the FLAC decoder
			var initResult = LibFLACSharp.FLAC__stream_decoder_init_stream(
				_decoderContext,
				_readCallback,
				_seekCallback,
				_tellCallback,
				_lengthCallback,
				_eofCallback,
				_writeCallback,
				_metadataCallback,
				_errorCallback);
            if (initResult != LibFLACSharp.StreamDecoderInitStatus.Ok)
				throw new InvalidOperationException("The flac decoder could not be intialized from the stream. The reason is " + initResult.ToString());

 			// Process the meta-data (but not the audio frames) so we can prepare the NAudio wave format
			FLACCheck(
				LibFLACSharp.FLAC__stream_decoder_process_until_end_of_metadata(_decoderContext),
				"The stream could not be processed until the end of the metadata");

			// Initialize NAudio wave format
			_waveFormat = new WaveFormat(_flacStreamInfo.SampleRate, _flacStreamInfo.BitsPerSample, _flacStreamInfo.Channels);
		}


		#region Init stream callback methods
		private LibFLACSharp.StreamDecoderReadStatus ReadCallback(IntPtr context, IntPtr bufferHandle, ref int byteCount, IntPtr clientData)
		{
			try
			{
				var buffer = new byte[byteCount];
				byteCount = _stream.Read(buffer, 0, byteCount);
				if (byteCount == 0)
				{
					return LibFLACSharp.StreamDecoderReadStatus.EndOfStream;
				}
				Marshal.Copy(buffer, 0, bufferHandle, byteCount);
			}
			catch (EndOfStreamException)
			{
				return LibFLACSharp.StreamDecoderReadStatus.EndOfStream;
			}
			catch (Exception)
			{
				return LibFLACSharp.StreamDecoderReadStatus.Abort;
			}

			return LibFLACSharp.StreamDecoderReadStatus.Continue;
		}

		private LibFLACSharp.StreamDecoderSeekStatus SeekCallback(IntPtr context, ulong absolute_byte_offset, IntPtr client_data)
		{
			try
			{
				_stream.Seek((long)absolute_byte_offset, SeekOrigin.Begin);
			}
			catch (NotSupportedException)
			{
				return LibFLACSharp.StreamDecoderSeekStatus.Unsupported;
			}
			catch (Exception)
			{
				return LibFLACSharp.StreamDecoderSeekStatus.Error;
			}

			return LibFLACSharp.StreamDecoderSeekStatus.Ok;
		}

		private LibFLACSharp.StreamDecoderTellStatus TellCallback(IntPtr context, ref ulong absolute_byte_offset, IntPtr client_data)
		{
			try
			{
				absolute_byte_offset = (ulong)_stream.Position;
			}
			catch (NotSupportedException)
			{
				return LibFLACSharp.StreamDecoderTellStatus.Unsupported;
			}
			catch (Exception)
			{
				return LibFLACSharp.StreamDecoderTellStatus.Error;
			}

			return LibFLACSharp.StreamDecoderTellStatus.Ok;
		}

		private LibFLACSharp.StreamDecoderLengthStatus LengthCallback(IntPtr context, ref ulong stream_length, IntPtr client_data)
		{
			try
			{
				stream_length = (ulong)_stream.Length;
			}
			catch (NotSupportedException)
			{
				return LibFLACSharp.StreamDecoderLengthStatus.Unsupported;
			}
			catch (Exception)
			{
				return LibFLACSharp.StreamDecoderLengthStatus.Error;
			}

			return LibFLACSharp.StreamDecoderLengthStatus.Ok;
		}

		private int EofCallback(IntPtr context, IntPtr client_data)
		{
			return EndOfStream ? 1 : 0;
		}


		/// <summary>
		/// FLAC Write Call Back - libFlac notifies back on a frame that was read from the source file and written as a frame
		/// </summary>
		/// <param name="context"></param>
		/// <param name="frame"></param>
		/// <param name="buffer"></param>
		/// <param name="clientData"></param>
		private LibFLACSharp.StreamDecoderWriteStatus WriteCallback(IntPtr context, IntPtr frame, IntPtr buffer, IntPtr clientData)
		{
			// Read the FLAC Frame into a memory samples buffer (_flacSamples)
			var flacFrame = (LibFLACSharp.FlacFrame)Marshal.PtrToStructure(frame, typeof(LibFLACSharp.FlacFrame));

			//if (_flacSamples == null)
			//{
				// First time - Create Flac sample buffer
				_samplesPerChannel = flacFrame.Header.BlockSize;
				_flacSamples = new int[_samplesPerChannel * _flacStreamInfo.Channels];
				_flacSampleIndex = 0;
			//}

			// Iterate on all channels, copy the unmanaged channel bits (samples) to the a managed samples array
			for (int inputChannel = 0; inputChannel < _flacStreamInfo.Channels; inputChannel++)
			{
				// Get pointer to channel bits, for the current channel
				var pChannelBits = Marshal.ReadIntPtr(buffer, inputChannel * IntPtr.Size);

				// Copy the unmanaged bits to managed memory
				Marshal.Copy(pChannelBits, _flacSamples, inputChannel * _samplesPerChannel, _samplesPerChannel);
			}

			lock (_repositionLock)
			{
				// Keep the current sample number for reporting purposes (See: Position property of FlacFileReader)
				_lastSampleNumber = flacFrame.Header.FrameOrSampleNumber;

				if (_repositionRequested)
				{
					_repositionRequested = false;
					FLACCheck(LibFLACSharp.FLAC__stream_decoder_seek_absolute(_decoderContext, _flacReposition), "Could not seek absolute: " + _flacReposition);
				}
			}
			return LibFLACSharp.StreamDecoderWriteStatus.Continue;
		}

		/// <summary>
		/// FLAC Meta Call Back - libFlac notifies about one (or more) Meta frames.
		/// Note that there could be many types of Meta Frames but by default only the StreamInfo meta frame is returned
		/// </summary>
		/// <param name="context"></param>
		/// <param name="metadata"></param>
		/// <param name="userData"></param>
		private void MetadataCallback(IntPtr context, IntPtr metadata, IntPtr userData)
		{
			var flacMetaData = (LibFLACSharp.FLACMetaData)Marshal.PtrToStructure(metadata, typeof(LibFLACSharp.FLACMetaData));

			if (flacMetaData.MetaDataType == LibFLACSharp.FLACMetaDataType.StreamInfo)
			{
				GCHandle pinnedStreamInfo = GCHandle.Alloc(flacMetaData.Data, GCHandleType.Pinned);
				try
				{
					_flacStreamInfo = (LibFLACSharp.FLACStreamInfo)Marshal.PtrToStructure(
						pinnedStreamInfo.AddrOfPinnedObject(),
						typeof(LibFLACSharp.FLACStreamInfo));
					_totalSamples = (long)(_flacStreamInfo.TotalSamplesHi << 32) + (long)_flacStreamInfo.TotalSamplesLo;
				}
				finally
				{
					pinnedStreamInfo.Free();
				}
			}
		}

		/// <summary>
		/// FLAC Error Call Back - libFlac notifies about a decoding error
		/// </summary>
		/// <param name="context"></param>
		/// <param name="status"></param>
		/// <param name="userData"></param>
		private void ErrorCallback(IntPtr context, LibFLACSharp.DecodeError status, IntPtr userData)
		{
			throw new ApplicationException(string.Format("FLAC: Could not decode frame: {0}!", status));
		}

		#endregion

		#region Overrides - Implement logic which is specific to FLAC

		/// <summary>
		/// This is the length in bytes of data available to be read out from the Read method
		/// (i.e. the decompressed FLAC length)
		/// n.b. this may return 0 for files whose length is unknown
		/// </summary>
		public override long Length
		{
			get
			{
				// Note: Workaround to fix NAudio calculation of position (which takes channels into account) and FLAC (which ignores channels for sample position)
				return (long)_totalSamples * _waveFormat.BlockAlign;
			}
		}

		/// <summary>
		/// <see cref="WaveStream.WaveFormat"/>
		/// </summary>
		public override WaveFormat WaveFormat
		{
			get { return _waveFormat; }
		}

		/// <summary>
		/// <see cref="Stream.Position"/>
		/// </summary>
		public override long Position
		{
			get
			{
				long lastSampleNumber;
				lock (_repositionLock)
				{
					// NOTE: FLAC__stream_decoder_get_decode_position() function returns byte index which is useless as it returns the position of the *uncompressed* decoded stream, not the compressed sample source position! 
					// Instead the last sample number of frame from write_callback is being used
					// See also: http://comments.gmane.org/gmane.comp.audio.compression.flac.devel/2252

					lastSampleNumber = _lastSampleNumber;
				}
				// Note: Adjust FLAW raw sample number to NAudio position (which takes block align into account)
				return lastSampleNumber * _waveFormat.BlockAlign;
			}
			set
			{
				lock (_repositionLock)
				{
					_flacSampleIndex = 0;

					// Note: Adjust NAudio position to FLAC sample number (which is raw and ignores takes block align)
					_repositionRequested = true;
					_flacReposition = value / (_waveFormat.BlockAlign);
					_lastSampleNumber = _flacReposition;
				}
			}
		}

		/// <summary>
		/// Reads decompressed PCM data from our FLAC file into the NAudio playback sample buffer
		/// </summary>
		/// <remarks>
		/// 1. The original code did not stop on end of stream. tomislavtustonic applied a fix using FLAC__stream_decoder_get_state. <seealso cref="https://code.google.com/p/practicesharp/issues/detail?id=14"/>
		/// </remarks>
		public override int Read(byte[] playbackSampleBuffer, int offset, int numBytes)
		{
			if (EndOfStream) return 0;		
			var flacBytesCopied = 0;

			_NAudioSampleBuffer = playbackSampleBuffer;
			_playbackBufferOffset = offset;

			lock (_repositionLock)
			{
				// If there are still samples in the flac buffer, use them first before reading the next FLAC frame
				if (_flacSampleIndex > 0)
				{
					flacBytesCopied = CopyFlacBufferToNAudioBuffer();
				}
			}
			var decoderState = LibFLACSharp.FLAC__stream_decoder_get_state(_decoderContext);
			// Keep reading flac packets until enough bytes have been copied
			while (flacBytesCopied < numBytes)
			{
				// Read the next PCM bytes from the FLAC File into the sample buffer
				FLACCheck(
						LibFLACSharp.FLAC__stream_decoder_process_single(_decoderContext),
						"process single");
				decoderState = LibFLACSharp.FLAC__stream_decoder_get_state(_decoderContext);
				if (decoderState == LibFLACSharp.StreamDecoderState.EndOfStream)
					break;
				else
					flacBytesCopied += CopyFlacBufferToNAudioBuffer();
			}

			return flacBytesCopied;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Helper utility function - Checks the result of a libFlac function by throwing an exception if the result was false
		/// </summary>
		/// <param name="result"></param>
		/// <param name="operation"></param>
		private void FLACCheck(bool result, string operation)
		{
			if (!result)
				throw new ApplicationException(string.Format("FLAC: Could not {0}!", operation));
		}

		/// <summary>
		/// Copies the Flac buffer samples to the NAudio buffer
		/// This method is an "Adapter" between the two different buffers and is the key functionality 
		///   that enables NAudio to play FLAC frames
		/// The Flac buffer has a different length and structure (i.e. all samples from channel 0, all samples from channel 1)
		///   than the NAudio samples buffer which has a interleaved structure (e.g sample 1 from channel 0, then sample 1 from channel 1 then sample 2 channel from Channel 1 etc.)
		/// </summary>
		/// <returns></returns>
		private int CopyFlacBufferToNAudioBuffer()
		{
			var startPlaybackBufferOffset = _playbackBufferOffset;
			var nAudioBufferFull = _playbackBufferOffset >= _NAudioSampleBuffer.Length;

			// For each channel, there are BlockSize number of samples, so let's process these.
			for (; _flacSampleIndex < _samplesPerChannel && !nAudioBufferFull; _flacSampleIndex++)
			{
				for (int channel = 0; channel < _flacStreamInfo.Channels && !nAudioBufferFull; channel++)
				{
					var sample = _flacSamples[_flacSampleIndex + channel * _samplesPerChannel];

					switch (_flacStreamInfo.BitsPerSample)
					{
					case 16: // 16-bit
						_NAudioSampleBuffer[_playbackBufferOffset++] = (byte)(sample);
						_NAudioSampleBuffer[_playbackBufferOffset++] = (byte)(sample >> 8);

						nAudioBufferFull = _playbackBufferOffset >= _NAudioSampleBuffer.Length;

						break;

					case 24: // 24-bit
							 // Note: Code contributed by Mathew1800, https://code.google.com/p/practicesharp/issues/detail?id=16#c2
						_NAudioSampleBuffer[_playbackBufferOffset++] = (byte)((sample >> 0) & 0xFF);
						_NAudioSampleBuffer[_playbackBufferOffset++] = (byte)((sample >> 8) & 0xFF);
						_NAudioSampleBuffer[_playbackBufferOffset++] = (byte)((sample >> 16) & 0xFF);

						nAudioBufferFull = _playbackBufferOffset >= _NAudioSampleBuffer.Length;
						break;

					default:
						throw new NotSupportedException("Input FLAC bit depth is not supported!");
					}
				}
			}

			// Flac buffer has been exhausted, reset the buffer sample index so it starts from the beginning
			if (_flacSampleIndex >= _samplesPerChannel)
			{
				_flacSampleIndex = 0;
			}

			// Return number of actual bytes copied
			var bytesCopied = _playbackBufferOffset - startPlaybackBufferOffset;
			return bytesCopied;
		}

		private bool EndOfStream => false; // _stream.Position == _stream.Length;

		#endregion

		#region Dispose

		/// <summary>
		/// Disposes this WaveStream
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				var context = Interlocked.Exchange(ref _decoderContext, IntPtr.Zero);
				if (context != IntPtr.Zero)
				{
					var finishResult = LibFLACSharp.FLAC__stream_decoder_finish(context);
					var deleteResult = LibFLACSharp.FLAC__stream_decoder_delete(context);
					FLACCheck(finishResult, "finalize stream decoder");
					FLACCheck(deleteResult, "dispose of stream decoder instance");
				}

				var stream = Interlocked.Exchange(ref _stream, null);
				if (stream != null)
				{
					stream.Close();
					stream.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Private Members

		private readonly WaveFormat _waveFormat;
		private readonly object _repositionLock = new object();

		private IntPtr _decoderContext;
		private Stream _stream;

		private LibFLACSharp.FLACStreamInfo _flacStreamInfo;
		private int _samplesPerChannel;
		private long _totalSamples = 0;

		private long _lastSampleNumber = 0;

		private int[] _flacSamples;
		private int _flacSampleIndex;

		private byte[] _NAudioSampleBuffer;
		private int _playbackBufferOffset;

		private bool _repositionRequested = false;
		private long _flacReposition = 0;

		private readonly LibFLACSharp.Decoder_WriteCallback _writeCallback;
		private readonly LibFLACSharp.Decoder_MetadataCallback _metadataCallback;
		private readonly LibFLACSharp.Decoder_ErrorCallback _errorCallback;
		private readonly LibFLACSharp.Decoder_StreamDecoderReadCallback _readCallback;
		private readonly LibFLACSharp.Decoder_StreamDecoderSeekCallback _seekCallback;
		private readonly LibFLACSharp.Decoder_StreamDecoderTellCallback _tellCallback;
		private readonly LibFLACSharp.Decoder_StreamDecoderLengthCallback _lengthCallback;
		private readonly LibFLACSharp.Decoder_StreamDecoderEofCallback _eofCallback;
		#endregion
	}
}