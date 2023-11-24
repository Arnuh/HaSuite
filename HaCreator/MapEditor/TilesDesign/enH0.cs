﻿/*
  koolk's Map Editor

  Copyright (c) 2009-2013 koolk

  This software is provided 'as-is', without any express or implied
  warranty. In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

     1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.

     2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.

     3. This notice may not be removed or altered from any source
     distribution.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HaCreator.MapEditor.TilesDesign {
	internal class enH0 : MapTileDesign {
		public enH0() {
			type = "enH0";

			potentials.Add(new MapTileDesignPotential("bsc", 0, 0));
			potentials.Add(new MapTileDesignPotential("enH0", -90, 0));
			potentials.Add(new MapTileDesignPotential("enH0", 90, 0));
			potentials.Add(new MapTileDesignPotential("enH1", 0, 0));
			potentials.Add(new MapTileDesignPotential("edU", 0, 0));
			potentials.Add(new MapTileDesignPotential("edU", 90, 0));
			potentials.Add(new MapTileDesignPotential("enV0", 90, -60));
			potentials.Add(new MapTileDesignPotential("enV1", 0, -60));
			potentials.Add(new MapTileDesignPotential("slLU", 0, 60));
			potentials.Add(new MapTileDesignPotential("slLU", 180, 0));
			potentials.Add(new MapTileDesignPotential("slRU", 90, 60));
			potentials.Add(new MapTileDesignPotential("slRU", -90, 0));
			potentials.Add(new MapTileDesignPotential("slLD", 90, 0));
			potentials.Add(new MapTileDesignPotential("slRD", 0, 0));
		}
	}
}