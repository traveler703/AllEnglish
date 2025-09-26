# AllEn

AllEn 是一个学习英语项目，这个仓库是我们的前端。前端是基于 Vue 3 和 Vite 开发的，旨在提供快速、响应式的用户体验。

## 技术栈

- **Vue 3** - 用于构建用户界面
- **Vite** - 用于构建和开发环境
- **Node.js** - 用于服务器端支持
- **cnpm** - 使用 **cnpm** 来提高 npm 安装依赖的速度（特别适用于中国大陆地区），我们用cnpm

## 环境配置

在运行本项目之前，你需要确保已正确配置以下环境：

1. **Node.js**：确保你的开发环境已经安装了 [Node.js](https://nodejs.org/)。可以通过以下命令检查 Node.js 是否已经安装：

   ```bash
   node -v
   ```

## 安装与运行

### 1. 克隆仓库

首先，你需要克隆这个项目到本地：

如果你没有配SSH(我推荐去配置一下):

```bash
https://github.com/Notyourbing/AllEnFrontEnd.git
```
如果配置了SSH:

```bash
git@github.com:Notyourbing/AllEnFrontEnd.git
```


### 2. 安装依赖

进入项目文件夹，并安装项目的所有依赖：

```bash
cd AllEnFrontEnd
cnpm install
```


### 3. 运行开发环境

安装依赖后，运行以下命令启动本地开发服务器：

```bash
cnpm run dev
```

这将启动一个本地开发服务器，比如 `http://localhost:3000` 。打开浏览器并访问该地址，应该能看到你的项目正在运行。


## 项目结构

```
AllEn/
├── public/            # 公共资源文件夹
├── src/               # 源代码文件夹
│   ├── assets/        # 静态资源
│   ├── components/    # Vue 组件
|   ├── api/           # api接口
│   ├── router/        # 路由配置
|   ├── views/         # 页面组件
│   └── main.ts        # 入口文件
├── vite.config.ts     # Vite 配置文件
└── package.json       # 项目信息及依赖
```

## 以后提交Pull Request的大致流程


1. Fork 这个仓库（只需要第一次做）
2. 创建一个新的分支
3. 提交你所做的更改
4. 创建一个 Pull Request，描述你的更改内容

