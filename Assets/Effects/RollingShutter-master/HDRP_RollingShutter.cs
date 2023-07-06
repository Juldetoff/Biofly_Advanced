using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Recorder.Examples;

public class HDRP_RollingShutter : MonoBehaviour
{
    public Camera sceneCamera;
    public Camera compositorCamera;
    public Material maskMaterial;
    public int temporalResolution = 10;
    public RenderTextureFormat textureFormat = RenderTextureFormat.RGB565;

    private CommandBuffer commandBuffer;
    public List<RenderTexture> renderTextures = new List<RenderTexture>();

    private void Start()
    {
        QualitySettings.vSyncCount = 2;
        Application.targetFrameRate = (int)sceneCamera.GetComponent<MovieRecordManual>().fps; //corrigera ptet la vitesse de la vidéo

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
    }

    private void Update()
    {
        renderTextures.Insert(0, renderTextures[renderTextures.Count - 1]);
        renderTextures.RemoveAt(renderTextures.Count - 1);

        sceneCamera.targetTexture = renderTextures[0];

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
    }

    public RenderTexture GetRenderTexture()
    {
        return sceneCamera.targetTexture;
    }

    public void ApplyRollingShutterEffect(RenderTexture source, RenderTexture destination)
    {
        commandBuffer.Clear();
        commandBuffer.SetGlobalFloat("_MaskPercentage", 1.0f);
        commandBuffer.Blit(source, destination, maskMaterial);
        Graphics.ExecuteCommandBuffer(commandBuffer);
    }

    public void UpdateRenderTexture(){
        Graphics.Blit(sceneCamera.targetTexture, renderTextures[0]);
    }

}