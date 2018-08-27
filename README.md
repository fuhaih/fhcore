# vs code 开发 .net core 程序
## vs code 插件
* c# ：提供语法高亮
* c# Extensions ：方便创建和管理文件
* NuGet Package Manager ：包管理工具
* Razor Snippets： Razor智能提示
## vs code 编译调试 .net core 程序
vs cod 使用的 .net core 编译器和调试器分别是 omnisharp 和 debugger，这两个会在 .net core 项目启动的时候进行下载

另外还需要用到两个配置文件launch.json和tasks.json，这两个也可以由vs code自动添加（vs code右下角会有提示添加）
## 常用快捷键
* ctrl+` :打开终端
* ctrl+p :打开顶部命令行

# 准备工作
安装 .net core sdk  
## 创建解决方案
vs code 中打开一个文件夹,打开终端(ctrl+`)
```
>dotnet new sln
```
会在文件夹中创建一个同名的sln文件
## 创建项目
```
>dotnet new mvc -o Host/FHCore.MVC
>dotnet new xunit -o Tests/FHCore.Test
```
直接会在打开的文件夹中创建一个名为FHCore.MVC 的 .net core mvc 项目
## 添加项目到解决方案并添加项目引用
```
>dotnet sln add Host/FHCore.MVC/FHCore.MVC.csproj
>dotnet sln add Tests/FHCore.Test/FHCore.Test.csproj
>dotnet add Tests/FHCore.Test/FHCore.Test.csproj reference Host/FHCore.MVC/FHCore.MVC.csproj
```

## 添加调试配置文件launch.json和tasks.json
launch.json在点击调试时会提示创建

tasks.json可以通过命令行创建    
打开命令行(ctrl+p)
```csharp
>Tasks:Configure Tasks 
-使用模板创建tasks.json文件
-.net core
```

然后修改launch.json文件
```json
"program": "${workspaceFolder}/Host/FHCore.MVC/bin/Debug/netcoreapp2.0/FHCore.MVC.dll",
"cwd": "${workspaceFolder}/Host/FHCore.MVC",
"sourceFileMap": {
                "/Views": "${workspaceFolder}/Host/FHCore.MVC//Views"
            }
```
## 安装依赖包
* 打开命令行（ctrl+p）
* 输入>nuget,选择安装包
* 输入包名（会模糊查询）按回车
* 选择要安装的包
* 选择版本号

## 指定mvc启动端口号
Program文件中修改
```csharp
public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                //指定使用Kestrel服务器
                //.UseKestrel()
                //.UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                //指定端口号
                .UseUrls("http://*:6606")
                .Build();
```
## 用bundle压缩静态资源文件
这个配置bundleconfig.json文件就行了
# 用autofac替换原生的IOC容器
## 安装autofac
通过nuget安装用于 .net core mvc 的autofac扩展库Autofac.Extensions.DependencyInjection
## 修改Startup
```csharp
public IContainer ApplicationContainer { get; private set; }
// This method gets called by the runtime. Use this method to add services to the container.
public IServiceProvider ConfigureServices(IServiceCollection services)
{
    services.AddMvc().AddControllersAsServices();
    var builder = new ContainerBuilder();
    builder.Populate(services);
    //让HomeController能通过属性进行注入
    builder.RegisterType<HomeController>().PropertiesAutowired();
    //builder.RegisterModule(new LoggingModule());
    //builder.RegisterType<MyType>().As<IMyType>();
    this.ApplicationContainer = builder.Build();
    // Create the IServiceProvider based on the container.
    return new AutofacServiceProvider(this.ApplicationContainer);
}
```

## 通过autofac注入log4net
在Program文件中配置log4net
```csharp
static Program()
{
    ILoggerRepository repository=LogManager.CreateRepository("console");
    FileInfo config=new FileInfo("log4net.config");
    log4net.Config.XmlConfigurator.ConfigureAndWatch(repository,config);
}
```
新建LoggingModule类
```csharp
public class LoggingModule: Autofac.Module
{
    private static void InjectLoggerProperties(object instance)
    {
        var instanceType = instance.GetType();

        // Get all the injectable properties to set.
        // If you wanted to ensure the properties were only UNSET properties,
        // here's where you'd do it.
        var properties = instanceType
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => p.PropertyType == typeof(ILog) && p.CanWrite && p.GetIndexParameters().Length == 0);

        // Set the properties located.
        foreach (var propToSet in properties)
        {
            propToSet.SetValue(instance, LogManager.GetLogger("console",instanceType), null);
        }
    }

    private static void OnComponentPreparing(object sender, PreparingEventArgs e)
    {
        e.Parameters = e.Parameters.Union(
        new[]
        {
            new ResolvedParameter(
                (p, i) => p.ParameterType == typeof(ILog),
                (p, i) => LogManager.GetLogger("console",p.Member.DeclaringType)
            ),
        });
    }

    protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
    {
        // Handle constructor parameters.
        // 处理构造器参数
        registration.Preparing += OnComponentPreparing;

        // Handle properties.
        //处理属性
        registration.Activated += (sender, e) => InjectLoggerProperties(e.Instance);
    }
}
```

进行依赖注入
```csharp
builder.RegisterModule(new LoggingModule());
```
## Multitenant Support
## [更多](http://autofac.readthedocs.io/en/latest/integration/aspnetcore.html)

# 单元测试

## 配置
在tasks.json中添加单元测试任务
```json
{
    // See https://go.microsoft.com/fwlink/?LinkId=733558
    // for the documentation about the tasks.json format
    "version": "2.0.0",
    "tasks": [
        {
            "label": "build",
            "command": "dotnet build",
            "type": "shell",
            "group": "build",
            "presentation": {
                "reveal": "silent"
            },
            "problemMatcher": "$msCompile"
        },
        {
            "label": "test",
            "command":"dotnet test ${workspaceFolder}/Tests/FHCore.Test/FHCore.Test.csproj",
            "type":"shell",
            "group": "test",
            "presentation": {
                "reveal": "silent"
            },
            "problemMatcher": "$msCompile"
        }
    ]
}
```

## 运行测试用例
任务->运行任务->选择test

## Moq
.net单元测试常用的mock框架，用来模拟一些外部对象，有时候我们要测试的一些方法中需要用到一些外部对象，就可以用mock框架来模拟这些外部对象
```csharp
[Fact]
public void TestIndexAction()
{
    var mockRepo = new Mock<ILog>();
    HomeController Controller=new HomeController(mockRepo.Object);
    IActionResult result= Controller.Index();
    Assert.True(true);
}
```
## coverlet
使用coverlet查看测试覆盖率

>通过nuget安装coverlet.msbuild 

>在json配置文件的dotnet test命令中添加/p:CollectCoverage=true

```json
{
    "label": "test",
    "command":"dotnet test ${workspaceFolder}/Tests/FHCore.Test/FHCore.Test.csproj /p:CollectCoverage=true",
    "type":"shell",
    "group": "test",
    "presentation": {
        "reveal": "silent"
    },
    "problemMatcher": "$msCompile"
}  
```

>在运行测试之后会在测试结果后面添加上测试覆盖率信息

# 发布(publish)
## 配置任务
在tasks.json中添加发布任务
```json
{
    "label": "publish",
    "command":"dotnet publish",
    "type": "shell",
    "group":"build",
    "presentation": {
        "reveal": "silent"
    },
    "problemMatcher": "$msCompile"       
}
```
如果需要发布到指定平台（如linux），可以在dotnet publish命令后添加参数来进行发布。
## 发布
任务->运行任务->选择publish

# Docker 发布
## 本地发布
`dotnet publish -c Release -o ./publish ${workspaceFolder}/Host/FHCore.MVC/FHCore.MVC.csproj`

发布后目录下会有publish文件夹，把文件夹拷贝到linux上

## 编写Dockerfile
```dockerfile
# microsoft/dotnet:2.1-aspnetcore-runtime镜像上搭建镜像
FROM microsoft/dotnet:2.1-aspnetcore-runtime
# 工作路径
WORKDIR /app
# 复制到工作路径
COPY ./publish /app
# 监听端口
EXPOSE 6606/tcp
# 由于文件都在工作路径上，所以可以直接dotnet FHCore.MVC.dll
# 如果dll不在工作路径上，需要加上路径，否则会误认为指令是SDK指令，提示安装SDK
ENTRYPOINT dotnet FHCore.MVC.dll
```

把Dockerfile拷贝到linux上的publish同级目录上。

## 创建镜像
```vim
docker build -t myapp/fhcore:1.0 .
```
最后的.是指Dockerfile在当前目录下，也可以指定Dockerfile

## 运行镜像
```vim
docker run --name fhcore -p 6606:6606 --restart always -d myapp/fhcore:1.0
```

端口号随意，映射的端口号（也就是docker内端口号）需要和WebHost定义的端口号一致。
