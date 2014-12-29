@ECHO OFF

ECHO Packing GNaP.Data.Scope.NHibernate
nuget pack src\GNaP.Data.Scope.NHibernate\GNaP.Data.Scope.NHibernate.csproj -Build -Prop Configuration=Release -Exclude gnap.ico
