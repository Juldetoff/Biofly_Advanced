using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using Cinemachine;

public class CineHDRP_RollingShutter : MonoBehaviour
{
    //public Camera sceneCamera;
    public CinemachineVirtualCamera sceneCamera;
    public Camera compositorCamera;
    public Material maskMaterial;
    public int temporalResolution = 100;
    public RenderTextureFormat textureFormat = RenderTextureFormat.RGB565;
    private RenderTexture virtualCameraTexture;

    private CommandBuffer commandBuffer;
    private List<RenderTexture> renderTextures = new List<RenderTexture>();

    private void Start()
    {
        QualitySettings.vSyncCount = 2;
        Application.targetFrameRate = 60;

        int w = Screen.width;
        int h = Screen.height;

        for (int i = 0; i < temporalResolution; i++)
        {
            RenderTexture renderTexture = new RenderTexture(w, h, 0, textureFormat);
            renderTexture.Create();
            renderTextures.Add(renderTexture);
        }

        commandBuffer = new CommandBuffer();
        commandBuffer.name = "Rolling shutter effect";

        // Vérifiez si le script est déjà attaché à la caméra compositor
        if (!compositorCamera.TryGetComponent<HDAdditionalCameraData>(out HDAdditionalCameraData additionalCameraData))
        {
            additionalCameraData = compositorCamera.gameObject.AddComponent<HDAdditionalCameraData>();
        }

        additionalCameraData.customRender += CustomRenderCallback;
        virtualCameraTexture = new RenderTexture(w, h, 0, textureFormat);
        virtualCameraTexture.Create();
    }

    private void Update()
    {
        renderTextures.Insert(0, renderTextures[renderTextures.Count - 1]);
        renderTextures.RemoveAt(renderTextures.Count - 1);

        //sceneCamera.targetTexture = renderTextures[0];
        RenderTexture tempTargetTexture = virtualCameraTexture;
        tempTargetTexture.Create();
        Graphics.SetRenderTarget(tempTargetTexture);

        commandBuffer.Clear();

        for (int i = 0; i < temporalResolution; i++)
        {
            commandBuffer.SetGlobalFloat("_MaskPercentage", (float)i / temporalResolution);
            commandBuffer.Blit(renderTextures[temporalResolution-1-i], BuiltinRenderTextureType.CameraTarget, maskMaterial);
        }
    }

    private void CustomRenderCallback(ScriptableRenderContext context, HDCamera hdCamera)
{
    context.ExecuteCommandBuffer(commandBuffer);
    commandBuffer.Clear();

    // Remettre la cible de rendu à la RenderTexture de la caméra virtuelle Cinemachine
    RenderTexture tempTargetTexture = virtualCameraTexture;
    Graphics.SetRenderTarget(tempTargetTexture);
}
}