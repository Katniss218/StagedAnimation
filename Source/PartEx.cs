using UnityEngine;
using System;
using System.Collections.Generic;

namespace StagedAnimation
{
	public static class PartEx
	{
		/// <summary>
		/// Returns the parent object of all the MODEL{} nodes.
		/// </summary>
		public static List<Transform> FindModelNodes( this Part part )
		{
			// Find the parent of all MODEL{} nodes.
			Transform modelParent = null;
			for( int i = 0; i < part.partTransform.childCount; i++ )
			{
				Transform child = part.partTransform.GetChild( i );
				if( child.gameObject.name == "model" )
				{
					modelParent = child;
					break;
				}
			}

			List<Transform> models = new List<Transform>();

			for( int i = 0; i < modelParent.childCount; i++ )
			{
				models.Add( modelParent.GetChild( i ) );
			}

			return models;
		}

		/// <summary>
		/// Returns an array of Animation objects on the specified part that have a specified name.
		/// </summary>
		public static Animation[] FindAnimation( this Part part, string animationName )
		{
			List<Transform> modelNodes = part.FindModelNodes();

			// find all animation modules
			List<Animation> foundAnims = new List<Animation>();
			foreach( var modelNode in modelNodes )
			{
				Part.FindModelComponents<Animation>( modelNode, string.Empty, foundAnims ); // this does a recursive search.
			}

			// get the animations that match the specified name.
			List<Animation> matchedAnims = new List<Animation>();
			foreach( var anim in foundAnims )
			{
				if( anim.GetClip( animationName ) == null )
				{
					continue;
				}

				matchedAnims.Add( anim );
			}

			return matchedAnims.ToArray();
		}
	}
}