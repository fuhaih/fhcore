layui.use('flow', function(){
    var flow = layui.flow;
    flow.load({
        elem: '#LAY_demo1' //流加载容器
        ,scrollElem: '#LAY_demo1' //滚动条所在元素，一般不用填，此处只是演示需要。
        ,isAuto: true
        ,isLazyimg: true
        ,done: function(page, next){ //加载下一页
          //模拟插入
          setTimeout(function(){
            var lis = [];
            for(var i = 0; i < 10; i++){
              lis.push('<li><img lay-src="/images/20141214093801_rdd2v.jpeg?v='+ ( (page-1)*10 + i + 1 ) +'"></li>')
            }
            next(lis.join(''), page < 6); //假设总页数为 6
          }, 500);
        }
      });
});