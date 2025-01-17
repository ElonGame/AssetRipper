﻿using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.ParticleSystemForceFieldParameters;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class ParticleSystemForceFieldParametersExtensions
	{
		public static ParticleSystemForceFieldShape GetShape(this IParticleSystemForceFieldParameters parameters)
		{
			return (ParticleSystemForceFieldShape)parameters.Shape;
		}
	}
}
