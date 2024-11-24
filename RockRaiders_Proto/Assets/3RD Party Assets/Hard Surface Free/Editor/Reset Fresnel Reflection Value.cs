using UnityEngine;
using UnityEditor;
using System.Collections;

// Set Fresnel Reflection Value to 1.13 range

public class MaterialValuesCopier : ScriptableObject
{
	private static Material mat;
	private static float frezvalue;
	

    [MenuItem ("Hard Surface / Set Fresnel Reflection Value to 1.13 range")]
    static void DoRecord()
    {
    	
    	foreach (Material m in Selection.GetFiltered(typeof(Material), SelectionMode.DeepAssets))
        {
#pragma warning disable CS0618 // Type or member is obsolete
            Undo.RegisterUndo (m, "Material Copy Change");
#pragma warning restore CS0618 // Type or member is obsolete

            if (m.HasProperty("_FrezPow"))
			{
        		frezvalue = m.GetFloat("_FrezPow");
				frezvalue *= 0.0009765625f;
				m.SetFloat("_FrezPow", frezvalue );
			}
		
		}
    }
 
   }
