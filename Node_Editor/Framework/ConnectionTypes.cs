﻿using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework
{
	public static class ConnectionTypes
	{
		public static Type NullType { get { return typeof(ConnectionTypes); } }
		
		// Static consistent information about types
		static Dictionary<string, TypeData> types = new Dictionary<string, TypeData> ();
		public static TypeData GetTypeData (string typeName)
		{
			if (types == null || types.Count == 0)
				NodeEditor.ReInit (false);
			TypeData typeData;
			if (!types.TryGetValue (typeName, out typeData))
			{
				Debug.LogError ("No TypeData defined for: " + typeName);
				typeData = types.First ().Value;
			}
			if (typeData.declaration == null || typeData.InputKnob == null || typeData.OutputKnob == null)
			{
				NodeEditor.ReInit (false);
				typeData = GetTypeData (typeName);
			}
			return typeData;
		}
		
		public static Type GetType (string typeName)
		{
			return GetTypeData (typeName).Type ?? NullType;
		}
		
		/// <summary>
		/// Fetches every Type Declaration in the assembly
		/// </summary>
		public static void FetchTypes () 
		{ // Search the current and (if the NodeEditor is packed into a .dll) the calling one
			types = new Dictionary<string, TypeData> ();

			List<Assembly> scriptAssemblies = AppDomain.CurrentDomain.GetAssemblies ().ToList ();
			if (!scriptAssemblies.Contains (Assembly.GetExecutingAssembly ()))
				scriptAssemblies.Add (Assembly.GetExecutingAssembly ());
			foreach (Assembly assembly in scriptAssemblies) 
			{
				if (!assembly.FullName.Contains ("Assembly"))
					continue;
				foreach (Type type in assembly.GetTypes ().Where (T => T.IsClass && !T.IsAbstract && T.GetInterfaces ().Contains (typeof (ITypeDeclaration)))) 
				{
					ITypeDeclaration typeDecl = assembly.CreateInstance (type.FullName) as ITypeDeclaration;
					if (typeDecl == null) 
					{
						Debug.LogError ("Error with Type Declaration " + type.FullName);
						return;
					}
					types.Add (typeDecl.name, new TypeData(typeDecl));
				}
			}
		}
	}

	public struct TypeData 
	{
		public ITypeDeclaration declaration;
		public Type Type;
		public Color col;
		public Texture2D InputKnob;
		public Texture2D OutputKnob;
		
		public TypeData (ITypeDeclaration typeDecl) 
		{
			declaration = typeDecl;
			Type = declaration.Type;
			col = declaration.col;

			InputKnob = ResourceManager.GetTintedTexture (declaration.InputKnob_TexPath, col);
			OutputKnob = ResourceManager.GetTintedTexture (declaration.OutputKnob_TexPath, col);
		}
	}

	public interface ITypeDeclaration
	{
		string name { get; }
		Color col { get; }
		string InputKnob_TexPath { get; }
		string OutputKnob_TexPath { get; }
		Type Type { get; }
	}

	// TODO: Node Editor: Built-In Connection Types
	public class FloatType : ITypeDeclaration 
	{
		public string name { get { return "Float"; } }
		public Color col { get { return Color.cyan; } }
		public string InputKnob_TexPath { get { return "Textures/In_Knob.png"; } }
		public string OutputKnob_TexPath { get { return "Textures/Out_Knob.png"; } }
		public Type Type { get { return typeof(float); } }
	}


}