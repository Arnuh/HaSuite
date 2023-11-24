/* -----------------------------------------------------------------------------

	Copyright (c) 2006 Simon Brown                          si@sjbrown.co.uk

	Permission is hereby granted, free of charge, to any person obtaining
	a copy of this software and associated documentation files (the
	"Software"), to	deal in the Software without restriction, including
	without limitation the rights to use, copy, modify, merge, publish,
	distribute, sublicense, and/or sell copies of the Software, and to
	permit persons to whom the Software is furnished to do so, subject to
	the following conditions:

	The above copyright notice and this permission notice shall be included
	in all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
	OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
	MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
	IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
	CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
	TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
	SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

   -------------------------------------------------------------------------- */

#include <squish.h>
#include "colourset.h"
#include "maths.h"
#include "rangefit.h"
#include "clusterfit.h"
#include "colourblock.h"
#include "alpha.h"
#include "singlecolourfit.h"

namespace squish {
	static int FixFlags(int flags) {
		// grab the flag bits
		int method = flags & (kDxt1 | kDxt3 | kDxt5);
		int fit = flags & (kColourIterativeClusterFit | kColourClusterFit | kColourRangeFit);
		int metric = flags & (kColourMetricPerceptual | kColourMetricUniform);
		int extra = flags & kWeightColourByAlpha;

		// set defaults
		if (method != kDxt3 && method != kDxt5)
			method = kDxt1;
		if (fit != kColourRangeFit)
			fit = kColourClusterFit;
		if (metric != kColourMetricUniform)
			metric = kColourMetricPerceptual;

		// done
		return method | fit | metric | extra;
	}

	void Compress(const u8* rgba, void* block, int flags) {
		// compress with full mask
		CompressMasked(rgba, 0xffff, block, flags);
	}

	void CompressMasked(const u8* rgba, int mask, void* block, int flags) {
		// fix any bad flags
		flags = FixFlags(flags);

		// get the block locations
		void* colourBlock = block;
		void* alphaBock = block;
		if ((flags & (kDxt3 | kDxt5)) != 0)
			colourBlock = reinterpret_cast<u8*>(block) + 8;

		// create the minimal point set
		ColourSet colours(rgba, mask, flags);

		// check the compression type and compress colour
		if (colours.GetCount() == 1) {
			// always do a single colour fit
			SingleColourFit fit(&colours, flags);
			fit.Compress(colourBlock);
		}
		else if ((flags & kColourRangeFit) != 0 || colours.GetCount() == 0) {
			// do a range fit
			RangeFit fit(&colours, flags);
			fit.Compress(colourBlock);
		}
		else {
			// default to a cluster fit (could be iterative or not)
			ClusterFit fit(&colours, flags);
			fit.Compress(colourBlock);
		}

		// compress alpha separately if necessary
		if ((flags & kDxt3) != 0)
			CompressAlphaDxt3(rgba, mask, alphaBock);
		else if ((flags & kDxt5) != 0)
			CompressAlphaDxt5(rgba, mask, alphaBock);
	}

	void Decompress(u8* rgba, const void* block_source, int flags) {
		// fix any bad flags
		flags = FixFlags(flags);

		// get the block locations
		const void* colourBlock = block_source;
		const void* alphaBock = block_source;
		if ((flags & (kDxt3 | kDxt5)) != 0)
			colourBlock = reinterpret_cast<const u8*>(block_source) + 8;

		// decompress colour
		DecompressColour(rgba, colourBlock, (flags & kDxt1) != 0);

		// decompress alpha separately if necessary
		if ((flags & kDxt3) != 0)
			DecompressAlphaDxt3(rgba, alphaBock);
		else if ((flags & kDxt5) != 0)
			DecompressAlphaDxt5(rgba, alphaBock);
	}

	int GetStorageRequirements(int width, int height, int flags) {
		// fix any bad flags
		flags = FixFlags(flags);

		// compute the storage requirements
		int blockcount = ((width + 3) / 4) * ((height + 3) / 4);
		int blocksize = ((flags & kDxt1) != 0) ? 8 : 16;
		return blockcount * blocksize;
	}

	void CompressImage(const u8* rgba, int width, int height, void* blocks, int flags) {
		// fix any bad flags
		flags = FixFlags(flags);

		// initialise the block output
		auto targetBlock = reinterpret_cast<u8*>(blocks);
		int bytesPerBlock = ((flags & kDxt1) != 0) ? 8 : 16;

		// loop over blocks
		for (int y = 0; y < height; y += 4) {
			for (int x = 0; x < width; x += 4) {
				// build the 4x4 block of pixels
				u8 sourceRgba[16 * 4];
				u8* targetPixel = sourceRgba;
				int mask = 0;
				for (int py = 0; py < 4; ++py) {
					for (int px = 0; px < 4; ++px) {
						// get the source pixel in the image
						int sx = x + px;
						int sy = y + py;

						// enable if we're in the image
						if (sx < width && sy < height) {
							// copy the rgba value
							const u8* sourcePixel = rgba + 4 * (width * sy + sx);
							for (int i = 0; i < 4; ++i)
								*targetPixel++ = *sourcePixel++;

							// enable this pixel
							mask |= (1 << (4 * py + px));
						}
						else {
							// skip this pixel as its outside the image
							targetPixel += 4;
						}
					}
				}

				// compress it into the output
				CompressMasked(sourceRgba, mask, targetBlock, flags);

				// advance
				targetBlock += bytesPerBlock;
			}
		}
	}

	void DecompressImage(u8* rgba, int width, int height, const void* blocks, int flags) {
		// fix any bad flags
		flags = FixFlags(flags);

		// initialise the block input
		auto sourceBlock = reinterpret_cast<const u8*>(blocks);
		int bytesPerBlock = ((flags & kDxt1) != 0) ? 8 : 16;

		// loop over blocks
		for (int y = 0; y < height; y += 4) {
			for (int x = 0; x < width; x += 4) {
				// decompress the block
				u8 targetRgba[4 * 16];
				Decompress(targetRgba, sourceBlock, flags);

				// write the decompressed pixels to the correct image locations
				const u8* sourcePixel = targetRgba;
				for (int py = 0; py < 4; ++py) {
					for (int px = 0; px < 4; ++px) {
						// get the target location
						int sx = x + px;
						int sy = y + py;
						if (sx < width && sy < height) {
							u8* targetPixel = rgba + 4 * (width * sy + sx);

							// copy the rgba value
							for (int i = 0; i < 4; ++i)
								*targetPixel++ = *sourcePixel++;
						}
						else {
							// skip this pixel as its outside the image
							sourcePixel += 4;
						}
					}
				}

				// advance
				sourceBlock += bytesPerBlock;
			}
		}
	}


	// DLL EXPORTS
	extern "C" {
	__declspec(dllexport) int _DLLEXPORT_FixFlags(int flags) {
		return FixFlags(flags);
	}

	__declspec(dllexport) void _DLLEXPORT_Compress(const u8* rgba, void* block, int flags) {
		Compress(rgba, block, flags);
	}

	__declspec(dllexport) void _DLLEXPORT_CompressMasked(const u8* rgba, int mask, void* block, int flags) {
		CompressMasked(rgba, mask, block, flags);
	}

	__declspec(dllexport) void _DLLEXPORT_Decompress(u8* rgba, const void* block_source, int flags) {
		Decompress(rgba, block_source, flags);
	}

	__declspec(dllexport) int _DLLEXPORT_GetStorageRequirements(int width, int height, int flags) {
		return GetStorageRequirements(width, height, flags);
	}

	__declspec(dllexport) void _DLLEXPORT_CompressImage(const u8* rgba, int width, int height, void* blocks_dest, int flags) {
		CompressImage(rgba, width, height, blocks_dest, flags);
	}

	__declspec(dllexport) void _DLLEXPORT_DecompressImage(u8* rgba, int width, int height, const void* blocks_source, int flags) {
		DecompressImage(rgba, width, height, blocks_source, flags);
	}
	}
}

// namespace squish
