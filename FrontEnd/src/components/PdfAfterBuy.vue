<template>
    <div class="pdf-container">
        <div class="pdf-viewer">
            <iframe v-if="showIframe"
                    :src="iframeSrc"
                    class="pdf-iframe"
                    frameborder="0"
                    @load="handleIframeLoad"
                    @error="handleIframeError"></iframe>
            <div v-else class="pdf-placeholder">
                {{ placeholderText }}
            </div>
        </div>
    </div>
</template>

<script>
    export default {
        name: 'PdfPreview',
        props: {
            pdfUrl: {
                type: String,
                required: true
            }
        },
        data() {
            return {
                showIframe: true,
                placeholderText: '请提供 PDF 文件 URL'
            }
        },
        computed: {
            iframeSrc() {
                if (this.pdfUrl.startsWith('http')) {
                    // Google Docs Viewer 固定第一页并隐藏所有控件
                    return `https://docs.google.com/gview?url=${encodeURIComponent(this.pdfUrl)}&embedded=true&rm=minimal&toolbar=0&navpanes=0&scrollbar=0&page=1&view=FitH`;
                } else {
                    // 本地文件直接显示
                    return this.pdfUrl;
                }
            }
        },
        methods: {
            handleIframeLoad() {
                // 屏蔽右键菜单防止下载
                const iframe = this.$el.querySelector('.pdf-iframe');
                if (iframe && iframe.contentDocument) {
                    iframe.contentDocument.oncontextmenu = () => false;
                }
                this.showIframe = true;
            },
            handleIframeError() {
                this.showIframe = false;
                this.placeholderText = '加载PDF失败';
            }
        }
    }
</script>

<style scoped>
    .pdf-container {
        height: 100%;
        width: 100%;
    }

    .pdf-viewer {
        height: 100%;
        width: 100%;
        overflow: hidden;
        background: #f0f0f0;
    }

    .pdf-iframe {
        width: 100%;
        height: 100%;
        min-height: 100vh; /* 确保填满整个可视区域 */
        border: none;
    }

    .pdf-placeholder {
        position: absolute;
        top: 50%;
        left: 50%;
        transform: translate(-50%, -50%);
        padding: 20px;
        background: #eee;
        border-radius: 4px;
        text-align: center;
    }
</style>