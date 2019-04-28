dotnet restore Smart.Reflection.Generative.CodeGenerator\Smart.Reflection.Generative.CodeGenerator.csproj
dotnet restore Smart.Reflection.Generative\Smart.Reflection.Generative.csproj

dotnet build Smart.Reflection.Generative.CodeGenerator\Smart.Reflection.Generative.CodeGenerator.csproj -c Release
dotnet build Smart.Reflection.Generative\Smart.Reflection.Generative.csproj -c Release
dotnet publish Smart.Reflection.Generative.CodeGenerator\Smart.Reflection.Generative.CodeGenerator.csproj -c Release -o bin\Release\PublishOutput

nuget pack Smart.Reflection.Generative.nuspec
