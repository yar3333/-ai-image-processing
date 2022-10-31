@pushd dist

@setlocal
set PATH=%~dp0..\_stuff\NVIDIA GPU Computing Toolkit_CUDA_v11.4\bin;%PATH%
set TORCH_HOME=%~dp0dist\stuff\models
set XDG_CACHE_HOME=%~dp0dist\stuff\models
invoke\aipainter_invokeai.exe --web %*
@endlocal

@popd
