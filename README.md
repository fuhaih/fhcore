# vs code 开发 .net core 程序

**注意:** 由于vs code对razor的自能提示不是很友好，所以直接换回vs studio进行开发

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

# 自定义静态文件
## 在Startup中配置静态文件
dotnet core中静态文件默认是在wwwroot目录下，其他目录下的文件是无法通过http访问的，如果有其他的静态文件夹需要通过http访问，可以自定义静态文件

```csharp
app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(
    Path.Combine(Directory.GetCurrentDirectory(), @"images")),
    RequestPath = new PathString("/images")
});
```
上面配置的静态文件夹是在项目目录下的images文件夹，通过[http://host/images/*]可以访问images目录下文件
## docker中添加数据卷
```vim
docker run --name fhcore -p 6606:6606 -v /home/dockerpublish/fhcore/images:/app/images --restart always -d myapp/fhcore:1.0
```
把主机/home/dockerpublish/fhcore/images目录映射到容器/app/images路径中，容器的工作路径是在/app，所以/app/images就是容器中项目的images路径了。
项目在访问images目录的时候，直接访问的是主机/home/dockerpublish/fhcore/images目录

# Docker部署

Docker支持直接用Dockerfile进行项目的部署,支持版本是1.7以上

修改Dockerfile内容如下
```dockerfile
FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 6606/tcp

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY Host/FHCore.MVC/FHCore.MVC.csproj Host/FHCore.MVC/
RUN dotnet restore Host/FHCore.MVC/FHCore.MVC.csproj
COPY . .
RUN dotnet build Host/FHCore.MVC/FHCore.MVC.csproj -c Release -o /app

FROM build AS publish
COPY Host/FHCore.MVC/layui /app/layui
RUN dotnet publish Host/FHCore.MVC/FHCore.MVC.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT dotnet FHCore.MVC.dll
```
**个人理解：** publish和final的虚拟空间是隔离开的，而base和final在同一个虚拟空间内，build和publish也在同一个虚拟空间内。所以最后的`COPY --from=publish /app .` 是指把publish的文件系统的/app目录下的文件复制到当前文件系统的工作路径下（`WORKDIR /app` 当前工作路径是/app目录）。

Dockerfile文件路径是在解决方案同目录下
这样方便自动化部署

# Docker可视化
```
docker run -d --privileged -p 9000:9000 -v /var/run/docker.sock:/var/run/docker.sock -v /opt/portainer:/data portainer/portainer
```

# 用bower包管理工具管理静态文件
> 安装node

windows下直接下载安装包安装

>安装bower

```node
$ npm install -g bower
```

>初始化bower

```
$ bower init 
```
会在当前项目根目录下多出一个bower.json的文件

> 添加bower配置文件

在当前项目的根目录中创建一个.bowerrc文件，在window下创建的话需要把文件名命名为".bowerrc."

配置信息如下

```json
{
    "directory" : "Host/FHCore.MVC/wwwroot/lib",
    "json"      : "",
    "endpoint"  : "",
    "searchpath"  : "",
    "shorthand_resolver" : ""
}
```
directory是包管理时的安装路径

>安装requirejs

```
$ bower install requirejs --save
```

# 配置

内存字典对象、json文件、init文件、xml文件

配置
```csharp
public class Program
{
    public static void Main(string[] args)
    {
        CreateWebHostBuilder(args).Build().Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                var dict = new Dictionary<string, string>
                {
                    {"test", "DEV_1111111-1111"},
                    {"test1", "PROD_2222222-2222"}
                };
                config.AddInMemoryCollection(dict);
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddIniFile("config.ini", optional: true, reloadOnChange: true);
                config.AddJsonFile("config.json", optional: true, reloadOnChange: true);
                config.AddXmlFile("config.xml", optional: true, reloadOnChange: true);
            })
            .UseStartup<Startup>();
}
```

使用
```csharp
public IConfiguration configuration;
public HomeController(IConfiguration configuration)
{
    this.configuration=configuration;
}

public IActionResult Index()
{
    string message= configuration.GetValue<string>("test");
    //ViewData["Message"] = "Your application description page.";
    ViewData["Message"] = message;
    return View();
}
```
