@if exist dist rmdir /s /q dist
@mkdir dist

dotnet publish -p:PublishSingleFile=true /p:DebugType=None -r win-x64 -c Release --self-contained false -o dist\AiPainter

if not exist dist\AiPainter\external                 mkdir dist\AiPainter\external
if not exist dist\AiPainter\external\lama-cleaner    mklink /D dist\AiPainter\external\lama-cleaner %~dp0..\_external\lama-cleaner\dist 2> nul
if not exist dist\AiPainter\external\ComfyUI        mklink /D dist\AiPainter\external\ComfyUI %~dp0..\_external\ComfyUI 2> nul

if not exist dist\AiPainter\stable_diffusion_checkpoints mklink /D dist\AiPainter\stable_diffusion_checkpoints %~dp0..\_stable_diffusion_checkpoints 2> nul
if not exist dist\AiPainter\stable_diffusion_styles      mklink /D dist\AiPainter\stable_diffusion_styles %~dp0..\_stable_diffusion_styles 2> nul
if not exist dist\AiPainter\stable_diffusion_vae         mklink /D dist\AiPainter\stable_diffusion_vae %~dp0..\_stable_diffusion_vae 2> nul
if not exist dist\AiPainter\stable_diffusion_lora        mklink /D dist\AiPainter\stable_diffusion_lora %~dp0..\_stable_diffusion_lora 2> nul
if not exist dist\AiPainter\stable_diffusion_embeddings  mklink /D dist\AiPainter\stable_diffusion_embeddings %~dp0..\_stable_diffusion_embeddings 2> nul
