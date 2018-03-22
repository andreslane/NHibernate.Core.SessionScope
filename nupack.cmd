@ECHO OFF

ECHO Packing NHibernate.SessionScope
src\.nuget\nuget pack src\GNaP.Data.Scope.NHibernate\GNaP.Data.Scope.NHibernate.csproj -Build -Prop Configuration=Release -Exclude gnap.ico
