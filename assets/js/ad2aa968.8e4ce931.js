"use strict";(self.webpackChunkdocs=self.webpackChunkdocs||[]).push([[26],{3905:function(t,e,n){n.d(e,{Zo:function(){return c},kt:function(){return m}});var r=n(7294);function a(t,e,n){return e in t?Object.defineProperty(t,e,{value:n,enumerable:!0,configurable:!0,writable:!0}):t[e]=n,t}function o(t,e){var n=Object.keys(t);if(Object.getOwnPropertySymbols){var r=Object.getOwnPropertySymbols(t);e&&(r=r.filter((function(e){return Object.getOwnPropertyDescriptor(t,e).enumerable}))),n.push.apply(n,r)}return n}function i(t){for(var e=1;e<arguments.length;e++){var n=null!=arguments[e]?arguments[e]:{};e%2?o(Object(n),!0).forEach((function(e){a(t,e,n[e])})):Object.getOwnPropertyDescriptors?Object.defineProperties(t,Object.getOwnPropertyDescriptors(n)):o(Object(n)).forEach((function(e){Object.defineProperty(t,e,Object.getOwnPropertyDescriptor(n,e))}))}return t}function l(t,e){if(null==t)return{};var n,r,a=function(t,e){if(null==t)return{};var n,r,a={},o=Object.keys(t);for(r=0;r<o.length;r++)n=o[r],e.indexOf(n)>=0||(a[n]=t[n]);return a}(t,e);if(Object.getOwnPropertySymbols){var o=Object.getOwnPropertySymbols(t);for(r=0;r<o.length;r++)n=o[r],e.indexOf(n)>=0||Object.prototype.propertyIsEnumerable.call(t,n)&&(a[n]=t[n])}return a}var s=r.createContext({}),u=function(t){var e=r.useContext(s),n=e;return t&&(n="function"==typeof t?t(e):i(i({},e),t)),n},c=function(t){var e=u(t.components);return r.createElement(s.Provider,{value:e},t.children)},d={inlineCode:"code",wrapper:function(t){var e=t.children;return r.createElement(r.Fragment,{},e)}},p=r.forwardRef((function(t,e){var n=t.components,a=t.mdxType,o=t.originalType,s=t.parentName,c=l(t,["components","mdxType","originalType","parentName"]),p=u(n),m=a,f=p["".concat(s,".").concat(m)]||p[m]||d[m]||o;return n?r.createElement(f,i(i({ref:e},c),{},{components:n})):r.createElement(f,i({ref:e},c))}));function m(t,e){var n=arguments,a=e&&e.mdxType;if("string"==typeof t||a){var o=n.length,i=new Array(o);i[0]=p;var l={};for(var s in e)hasOwnProperty.call(e,s)&&(l[s]=e[s]);l.originalType=t,l.mdxType="string"==typeof t?t:a,i[1]=l;for(var u=2;u<o;u++)i[u]=n[u];return r.createElement.apply(null,i)}return r.createElement.apply(null,n)}p.displayName="MDXCreateElement"},8957:function(t,e,n){n.r(e),n.d(e,{assets:function(){return c},contentTitle:function(){return s},default:function(){return m},frontMatter:function(){return l},metadata:function(){return u},toc:function(){return d}});var r=n(7462),a=n(3366),o=(n(7294),n(3905)),i=["components"],l={},s="Getting Started",u={unversionedId:"Getting-Started",id:"Getting-Started",title:"Getting Started",description:"Database Provisioning",source:"@site/docs/Getting-Started.md",sourceDirName:".",slug:"/Getting-Started",permalink:"/zytonomy/docs/Getting-Started",draft:!1,editUrl:"https://github.com/Thinktastic/zytonomy/tree/main/docs/docs/Getting-Started.md",tags:[],version:"current",frontMatter:{},sidebar:"tutorialSidebar",previous:{title:"Documentation",permalink:"/zytonomy/docs/"}},c={},d=[{value:"Database Provisioning",id:"database-provisioning",level:2},{value:"Configuration Notes",id:"configuration-notes",level:2},{value:"Running",id:"running",level:2},{value:"Start Azurite",id:"start-azurite",level:3},{value:"Start the Runtime",id:"start-the-runtime",level:3},{value:"Start the Frontend",id:"start-the-frontend",level:3},{value:"CosmosDB Name",id:"cosmosdb-name",level:3}],p={toc:d};function m(t){var e=t.components,n=(0,a.Z)(t,i);return(0,o.kt)("wrapper",(0,r.Z)({},p,n,{components:e,mdxType:"MDXLayout"}),(0,o.kt)("h1",{id:"getting-started"},"Getting Started"),(0,o.kt)("h2",{id:"database-provisioning"},"Database Provisioning"),(0,o.kt)("p",null,"The following instructions are used for provisioning the CosmosDB database on th localhost:"),(0,o.kt)("pre",null,(0,o.kt)("code",{parentName:"pre"},'az cosmosdb database create --db-name Zytonomy --key "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==" --url-connection "https://localhost:8081"\n\naz cosmosdb collection create --db-name Zytonomy --collection-name Core --key "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==" --url-connection "https://localhost:8081" --partition-key-path /PartitionKey\n')),(0,o.kt)("h2",{id:"configuration-notes"},"Configuration Notes"),(0,o.kt)("table",null,(0,o.kt)("thead",{parentName:"table"},(0,o.kt)("tr",{parentName:"thead"},(0,o.kt)("th",{parentName:"tr",align:null},"Setting"),(0,o.kt)("th",{parentName:"tr",align:null},"Notes"))),(0,o.kt)("tbody",{parentName:"table"},(0,o.kt)("tr",{parentName:"tbody"},(0,o.kt)("td",{parentName:"tr",align:null},(0,o.kt)("inlineCode",{parentName:"td"},"AzureWebJobsStorage")),(0,o.kt)("td",{parentName:"tr",align:null},"This setting should use the local storage for development otherwise it will not signal correctly.")),(0,o.kt)("tr",{parentName:"tbody"},(0,o.kt)("td",{parentName:"tr",align:null},(0,o.kt)("inlineCode",{parentName:"td"},"ContentSourceStorage")),(0,o.kt)("td",{parentName:"tr",align:null},"This setting must use the remote storage even for development because the URL needs to be visible to Q&A Maker")))),(0,o.kt)("h2",{id:"running"},"Running"),(0,o.kt)("h3",{id:"start-azurite"},"Start Azurite"),(0,o.kt)("p",null,"To run with Azurite, perform the following commands:"),(0,o.kt)("pre",null,(0,o.kt)("code",{parentName:"pre"},"cd api/zytonomy.api/azurite\nazurite -s                    # To start in silent mode; it's noisy otherwise.\n")),(0,o.kt)("p",null,"It is necessary to run this before starting the functions runtime."),(0,o.kt)("h3",{id:"start-the-runtime"},"Start the Runtime"),(0,o.kt)("p",null,"Execute the following commands to start the backend."),(0,o.kt)("pre",null,(0,o.kt)("code",{parentName:"pre"},"cd api/zytonomy.api\ndotnet restore                # Restore nuget packages (1st run)\ndotnet build                  # Build to make sure everything is OK (1st run)\nfunc start                    # Start the runtime\n")),(0,o.kt)("p",null,"You may need to reload VS Code after ",(0,o.kt)("inlineCode",{parentName:"p"},"dotnet restore"),".  Use ",(0,o.kt)("inlineCode",{parentName:"p"},"CTRL+SHIFT+P"),' and type "Reload" to find the command.'),(0,o.kt)("p",null,"To start with hot reload"),(0,o.kt)("pre",null,(0,o.kt)("code",{parentName:"pre"},"dotnet watch msbuild /t:RunFunctions\n")),(0,o.kt)("h3",{id:"start-the-frontend"},"Start the Frontend"),(0,o.kt)("p",null,"To start the front, end, run:"),(0,o.kt)("pre",null,(0,o.kt)("code",{parentName:"pre"},"cd web\nyarn                          # Restore packages (1st run)\nyarn dev                      # Start the runtime\n")),(0,o.kt)("p",null,"The application should be available at ",(0,o.kt)("inlineCode",{parentName:"p"},"http://localhost:3000")),(0,o.kt)("h3",{id:"cosmosdb-name"},"CosmosDB Name"),(0,o.kt)("p",null,"The CosmosDB name is configured in two places:"),(0,o.kt)("ol",null,(0,o.kt)("li",{parentName:"ol"},"In the ",(0,o.kt)("inlineCode",{parentName:"li"},"local.settings.json"),", the ",(0,o.kt)("inlineCode",{parentName:"li"},"Env_Suffix")," variable holds an environment variable name."),(0,o.kt)("li",{parentName:"ol"},"When the value is present, it will be appended in ",(0,o.kt)("inlineCode",{parentName:"li"},"CosmosGateway.cs")," to create the database name.")),(0,o.kt)("p",null,"This allows development against a shared CosmosDB instance if a local one is not available (macOS)"))}m.isMDXComponent=!0}}]);