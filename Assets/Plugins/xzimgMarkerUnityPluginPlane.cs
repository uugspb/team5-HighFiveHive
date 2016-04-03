/**
*
* Copyright (c) 2013 xzimg , All Rights Reserved
* No part of this software and related documentation may be used, copied,
* modified, distributed and transmitted, in any form or by any means,
* without the prior written permission of xzimg
*
* the xzimg company is located at 76 rue Gabriel PГ©ri - 78800 Houilles - FRANCE
* contact@xzimg.com, xzimg.com
*
*/

using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;	//List

public class xzimgMarkerUnityPluginPlane : MonoBehaviour 
{	
	
// Import marker detection functions
#if UNITY_IPHONE    
	[DllImport ("__Internal")] 
#else 
	[DllImport("xzimg-Marker-SDK-Unity")] 
#endif 
    private static extern bool xzimgMarkerInitialize(int CaptureWidth, int CaptureHeight, int iProcessingWidth, int iProcessingHeight, float fFOV);
 
#if UNITY_IPHONE    
	[DllImport ("__Internal")] 
#else 
	[DllImport("xzimg-Marker-SDK-Unity")] 
#endif 
    private static extern void xzimgMarkerRelease();

#if UNITY_IPHONE    
	[DllImport ("__Internal")] 
#else 
	[DllImport("xzimg-Marker-SDK-Unity")] 
#endif 
    private static extern void xzimgSetActiveIndices(ref int arrIndices, int nbrOfIndices);
	
#if UNITY_IPHONE    
	[DllImport ("__Internal")] 
#else 
	[DllImport("xzimg-Marker-SDK-Unity")] 
#endif 
    private static extern bool xzimgMarkerDetect([In][Out] ref stpImage imageIn, int markerSize, bool useFilter, int filterStrenght);

#if UNITY_IPHONE    
	[DllImport ("__Internal")] 
#else 
	[DllImport("xzimg-Marker-SDK-Unity")] 
#endif 
    private static extern int xzimgMarkerGetNumber();

#if UNITY_IPHONE    
	[DllImport ("__Internal")] 
#else 
	[DllImport("xzimg-Marker-SDK-Unity")] 
#endif 
    private static extern void xzimgMarkerGetInfoForUnity(int iId, [In][Out] ref xzimgMarkerInfoForUnity markerInfo);
	
#if UNITY_IPHONE    
	[DllImport ("__Internal")] 
#else 
	[DllImport("xzimg-Marker-SDK-Unity")] 
#endif 
    private static extern bool xzimgMarkerGetProtectionAlert();
	
	 // Create the marker structure to get marker's location according to the camera
	[StructLayout(LayoutKind.Sequential)]
    public struct xzimgMarkerInfoForUnity
	{
		public int markerID;
        public Vector3 position;
        public Vector3 euler;
        public Quaternion rotation;
	}
    private xzimgMarkerInfoForUnity markerInfo;
	
    // Create the image structure to push image data to the tracking
    [StructLayout(LayoutKind.Sequential)]
    public struct stpImage
    {
        public int  m_width;
        public int  m_height;
       	
		public IntPtr m_imageData;

	    /** 0: Black and White, 1: Color RGB, 2: Color BGR, 3: Color RGBA, 4: Color ARGB */
	    public int m_colorType;

	    /** 0: unsigned char, 1: float, 2: double */
	    public int m_type;
		
		/** Has the image to be flipped horinzontally */
		public bool m_flippedHorizontaly;
		
		/**  Id of the current frame*/
		public int m_frameId;
    }
    private stpImage ImageIn;

    private float aspect;

    public Transform localHandContainer;
    public Transform worldHandContainer;

	// Size of the video capture (constant)
    public int CaptureWidth = 640, CaptureHeight = 480;
	// Size of the image for marker detection
    public int ProcessingWidth = 640, ProcessingHeight = 480;
	public bool MirrorVideo = false;
	public float CameraFOVX = 45.0f;
	
	// to filter object pose
	public bool RecursiveFilter = false;
	public int FilterStrength = 1;
	
	// Size of the marker to be detected (choose between 2, 3, 4 or 5)
	public int MarkerSize = 5;
	public bool StretchRendering = false;
	public List<int> TrackOnlyIndices;
	
	// private variables
	private int VideoPlaneDistance = 750;
    //private GameObject m_3dGameObjectPivot1, m_3dGameObjectCG1;

    //private GameObject leftHand;
    //private GameObject rightHand;

    public GameObject leftHandPivot;
    public GameObject rightHandPivot;

    private GUIText m_GuiText;
	private WebCamTexture m_WebcamTexture;
	private Color32[] m_data;
	private WebCamDevice[] devices;
	private String deviceName;
    private GCHandle m_PixelsHandle;
	
    IEnumerator Start () 
	{
		//Camera.main.clearFlags = CameraClearFlags.Skybox;
		//Camera.main.transform.position = new Vector3(0, 0, 0);
		//Camera.main.transform.eulerAngles = new Vector3(0, 0, 0);
		transform.position = new Vector3(0, 0, 0);
		
		yield return Application.RequestUserAuthorization (UserAuthorization.WebCam | UserAuthorization.Microphone);
		if (Application.HasUserAuthorization(UserAuthorization.WebCam | UserAuthorization.Microphone)) 
		{
	        devices = WebCamTexture.devices;
       		deviceName = devices[0].name;
            m_WebcamTexture = new WebCamTexture(deviceName, CaptureWidth, CaptureHeight, 30);
            float aspect_ratio = (float)m_WebcamTexture.requestedWidth / (float)m_WebcamTexture.requestedHeight;
            Camera.main.fieldOfView = CameraFOVX / (aspect_ratio);
			
			GameObject goDisplayInfo = new GameObject("DisplayInfo");
			GUIText m_GuiText2 = (GUIText)goDisplayInfo.AddComponent(typeof(GUIText));
			m_GuiText2.transform.position = new Vector3(0.05f, 0.95f, 0);
			
			m_GuiText2.fontStyle = FontStyle.Bold;
			m_GuiText2.text = "Screen: " + Screen.width + "x" + Screen.height + " - " + m_WebcamTexture.requestedWidth + "x" + m_WebcamTexture.requestedHeight;
			
			
	        if (!m_WebcamTexture)
	            Debug.Log("No camera detected!");
			else
			{
				m_WebcamTexture.Play();
				
		        // Assign video texture to the renderer
		        if (GetComponent<Renderer>())
		        {
					// Modify Game Object's position & orientation according to the main camera's focal
			        transform.localPosition = new Vector3(0, 0, VideoPlaneDistance);
			        transform.eulerAngles = new Vector3(270, 0, 0);
					double tan_fov_rad_h = Math.Tan((double)(Camera.main.fieldOfView*aspect_ratio) / 2.0 * (Math.PI / 180.0));
					double tan_fov_rad_v = Math.Tan((double)Camera.main.fieldOfView / 2.0 * (Math.PI / 180.0));
					
                    double scale_u = (2.0f * (float)VideoPlaneDistance * tan_fov_rad_h);
			        double scale_v = (2.0f * (float)VideoPlaneDistance * tan_fov_rad_v);
					
					if (MirrorVideo)
			            transform.localScale = new Vector3((float)scale_u/10.0f, (float)1, (float)-scale_v/(9.8f));
			        else
			            transform.localScale = new Vector3((float)-scale_u/10.0f, (float)1, (float)-scale_v/(9.8f));

                    //transform.localScale *= aspect_ratio;
	
					/*GetComponent<Renderer>().material = new Material(
						"Shader \"Simple\" {" +
				        "SubShader {" +
				        "    Pass {" +
				        "        Color (1,1,1,0) Material { Diffuse (1,1,1,0) Ambient (1,1,1,0) }" +
				        "        Lighting Off" +
				        "        SetTexture [_MainTex]" +
				        "    }" +
				        "}" +
				        "}"
						);*/		
	
		        }
		        else
				{
		            Debug.Log("No renderer available for this object!");
				}
				
                xzimgMarkerInitialize(m_WebcamTexture.requestedWidth, m_WebcamTexture.requestedHeight, ProcessingWidth, ProcessingHeight, Camera.main.fieldOfView * aspect_ratio);

                /*
				// Find the gameobjects for pivot and 3D model
		        m_3dGameObjectPivot1 = GameObject.Find("ScenePivot");
		        m_3dGameObjectCG1 = GameObject.Find("SceneObject");
                */

                //////////////////////////////

                //leftHand = leftHandPivot.transform.GetChild(0).gameObject;
                //rightHand = rightHandPivot.transform.GetChild(0).gameObject;

                //////////////////////////////

                /*GameObject go = new GameObject("MarkerInfo");
				m_GuiText = (GUIText)go.AddComponent(typeof(GUIText));
				m_GuiText.fontStyle = FontStyle.Bold;
				m_GuiText.transform.position = new Vector3(0.05f, 0.90f, 0);*/
			}
			
			// Restore a camera fov that takes the screen width and height into account
			if (StretchRendering) Camera.main.aspect = aspect_ratio;
			
			if (TrackOnlyIndices.Count>0)
			{
				int [] arrIndices = new int[TrackOnlyIndices.Count];
				for (int i=0; i<TrackOnlyIndices.Count; i++)
					arrIndices[i] = TrackOnlyIndices[i];
				xzimgSetActiveIndices(ref arrIndices[0], TrackOnlyIndices.Count);
			}
			
			// Image structure
            ImageIn.m_width = m_WebcamTexture.requestedWidth;
            ImageIn.m_height = m_WebcamTexture.requestedHeight;
            ImageIn.m_colorType = 3;
            ImageIn.m_type = 0;
			ImageIn.m_flippedHorizontaly = true;
			
    		m_data = new Color32[m_WebcamTexture.requestedWidth * m_WebcamTexture.requestedHeight];
			m_PixelsHandle = GCHandle.Alloc(m_data, GCHandleType.Pinned);
            GetComponent<Renderer>().material.mainTexture = m_WebcamTexture;

            aspect = aspect_ratio;
		}

        
    }
    
    void OnDisable() 
	{
		xzimgMarkerRelease();
		m_PixelsHandle.Free();
    }
  
    void Update () 
	{
        if (m_WebcamTexture && m_WebcamTexture.didUpdateThisFrame) 
		{
            m_WebcamTexture.GetPixels32(m_data);
			
            /*
	        // Reset rendering
	        if (leftHand)
	        {
	            Renderer[] renderers = leftHand.GetComponentsInChildren<Renderer>();
	            foreach (Renderer r in renderers) r.enabled = false;
	        }
            if (rightHand)
            {
                Renderer[] renderers = rightHand.GetComponentsInChildren<Renderer>();
                foreach (Renderer r in renderers) r.enabled = false;
            }*/

            ImageIn.m_imageData = m_PixelsHandle.AddrOfPinnedObject();
			xzimgMarkerDetect(ref ImageIn, MarkerSize, RecursiveFilter, FilterStrength);
			bool ProtectionFailed = xzimgMarkerGetProtectionAlert();
			

	        int iNbrOfDetection = xzimgMarkerGetNumber();
	        if (iNbrOfDetection > 0)
	        {
	            for (int i = 0; i < iNbrOfDetection; i++)
	            {
	                xzimgMarkerGetInfoForUnity(i, ref markerInfo);
	                if (markerInfo.markerID == 0)
	                {
                        leftHandPivot.transform.SetParent(localHandContainer, true);

	                    //Renderer[] renderers = leftHand.GetComponentsInChildren<Renderer>();
	                    //foreach (Renderer r in renderers) r.enabled = true;
						
						Vector3 position = markerInfo.position;
						Quaternion quat = Quaternion.Euler(markerInfo.euler);
						/*if (MirrorVideo)
						{
							quat.y = -quat.y;
							quat.z = -quat.z;
							position.x = -position.x;
						}*/
	                    leftHandPivot.transform.localPosition = /*0.1f **/ position;
						leftHandPivot.transform.localRotation = quat;

                        

                        leftHandPivot.transform.SetParent(worldHandContainer, true);
                    }
                    if (markerInfo.markerID == 2)
                    {
                        rightHandPivot.transform.SetParent(localHandContainer, true);

                        //Renderer[] renderers = rightHand.GetComponentsInChildren<Renderer>();
                        //foreach (Renderer r in renderers) r.enabled = true;

                        Vector3 position = markerInfo.position;
                        Quaternion quat = Quaternion.Euler(markerInfo.euler);
                        /*if (MirrorVideo)
						{
							quat.y = -quat.y;
							quat.z = -quat.z;
							position.x = -position.x;
						}*/
                        rightHandPivot.transform.localPosition = /*0.1f * */position;
                        rightHandPivot.transform.localRotation = quat;

                        rightHandPivot.transform.SetParent(worldHandContainer, true);
                    }

                    /*if (m_GuiText)
	                { 
	                    // Display recognized object index
						if (!ProtectionFailed)
							m_GuiText.text = "Marker ID " + markerInfo.markerID;
						else
						{
							m_GuiText.fontStyle = FontStyle.Bold;
							m_GuiText.text = "Protection Error, Please Restart your Engine and Ensure that the XZIMG logo is visible";
						}
	                }*/
	            }
	        }
			/*else if (m_GuiText)
			{
				if (!ProtectionFailed)
					m_GuiText.text = "Marker ID " ;
				else
				{
					m_GuiText.fontStyle = FontStyle.Bold;
					m_GuiText.text = "Protection Error, Please Restart your Engine and Ensure that the XZIMG logo is visible";
				}
			}*/
		}
    }    

    
}

