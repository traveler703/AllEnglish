<template>
    <div class="crossword-layout">
        <!-- 顶部标题 -->
        <div class="page-header">
            <h1>填词游戏</h1>
        </div>

        <!-- 左右内容区域 -->
        <div class="main-content">
            <!-- 左侧题目列表 -->
            <div class="left-section">
                <h3>提示</h3>
                <div class="clues-container">
                    <div class="clue-item" v-for="clue in selectedclues" :key="clue.clueId">
                        {{ clue.clueDescription }}
                    </div>
                </div>
                <div class="button-group">
                    <el-button type="primary" @click="checkAnswers">提交答案</el-button>
                    <el-button type="warning" @click="loadNewPuzzle">更换题目</el-button>
                </div>
            </div>

            <!-- 右侧填字棋盘 -->
            <div class="right-section">
                <h3>题目</h3>
                <div class="grid-container">
                    <div class="grid">
                        <div v-for="(row, rowIndex) in grid" :key="rowIndex" class="grid-row">
                            <div v-for="(cell, colIndex) in row"
                                 :key="colIndex"
                                 class="grid-cell"
                                 :class="{
                  'black-cell': solution[rowIndex][colIndex] === '#',
                  correct: isCorrect(rowIndex, colIndex),
                  wrong: isWrong(rowIndex, colIndex)
                }">
                                <input v-if="solution[rowIndex][colIndex] !== '#'"
                                       v-model="grid[rowIndex][colIndex]"
                                       maxlength="1"
                                       class="grid-input" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
    import { ref, onMounted } from "vue";
    import { ElMessage } from "element-plus";
    import api from '../utils/axios'

    // 数据
    const PuzzlesData = ref([]);
    const selectedclues = ref([]);
    const grid = ref([]);
    const solution = ref([]);
    const correctCells = ref(new Set());
    const wrongCells = ref(new Set());

    // 初始化空棋盘
    function initEmptyGrid(size = 5) {
        const empty = Array.from({ length: size }, () => Array(size).fill('#'));
        grid.value = JSON.parse(JSON.stringify(empty));
        solution.value = JSON.parse(JSON.stringify(empty));
    }

    // 加载新题目
    async function loadNewPuzzle() {
        if (!PuzzlesData.value || PuzzlesData.value.length === 0) return;

        // 清空棋盘
        initEmptyGrid();

        // 随机选择题目
        const randomIndex = Math.floor(Math.random() * PuzzlesData.value.length);
        const temp_id = PuzzlesData.value[randomIndex].puzzleId;

        const res = await api.get('/api/game/get-clue', { params: { id: temp_id } });
        selectedclues.value = res.data.clues;

        // 填充棋盘
        for (const clue of selectedclues.value) {
            const randomIndex = Math.floor(Math.random() * clue.answer.length); 
            if (clue.direction === "Across") {
                for (let j = 0; j < clue.answer.length; j++) {
                    solution.value[clue.beginH - 1][clue.beginL - 1 + j] = clue.answer[j];
                    grid.value[clue.beginH - 1][clue.beginL - 1 + j] = j === randomIndex ? clue.answer[j] : "";
                }
            } else {
                for (let j = 0; j < clue.answer.length; j++) {
                    solution.value[clue.beginH - 1 + j][clue.beginL - 1] = clue.answer[j];
                    grid.value[clue.beginH - 1 + j][clue.beginL - 1] = j === randomIndex ? clue.answer[j] : "";
                }
            }
        }

        // 清空状态
        correctCells.value.clear();
        wrongCells.value.clear();
    }

    // 检查答案
    function checkAnswers() {
        correctCells.value.clear();
        wrongCells.value.clear();
        let allCorrect = true;

        for (let r = 0; r < solution.value.length; r++) {
            for (let c = 0; c < solution.value[r].length; c++) {
                if (solution.value[r][c] !== "#") {
                    const input = (grid.value[r][c] || "").toUpperCase();
                    if (input === solution.value[r][c]) {
                        correctCells.value.add(`${r}-${c}`);
                    } else {
                        wrongCells.value.add(`${r}-${c}`);
                        allCorrect = false;
                    }
                }
            }
        }

        if (allCorrect) {
            ElMessage.success("全部正确！恭喜！");
            setTimeout(loadNewPuzzle, 800);
        } else {
            ElMessage.warning("有答案错误，请检查！");
        }
    }

    // 工具函数
    function isCorrect(r, c) {
        return correctCells.value.has(`${r}-${c}`);
    }
    function isWrong(r, c) {
        return wrongCells.value.has(`${r}-${c}`);
    }

    // 页面加载时初始化
    onMounted(async () => {
        const res = await api.get('/api/game/get-puzzle');
        PuzzlesData.value = res.data.puzzles;
        await loadNewPuzzle();
    });
</script>

<style>
    .crossword-layout {
        width: 70%;
        margin: -90px auto 20px auto;
        display: flex;
        flex-direction: column; /* 纵向排列：标题在上，左右内容在下 */
        gap: 20px;
        padding: 60px 20px;
        background-color: #e6f7ff;
        border-radius: 20px;
        box-shadow: 0 2px 8px rgba(0,0,0,0.05);
    }

    .page-header {
        text-align: center;
        padding: 20px 0;
    }

        .page-header h1 {
            margin: 0;
            font-size: 32px;
            color: #ff66b3;
        }

    /* 左右内容区域 */
    .main-content {
        display: flex; /* 水平排列左右两栏 */
        gap: 20px;
    }

    /* 左侧题目 */
    .left-section {
        flex: 1;
        background: #fff;
        border-radius: 16px;
        padding: 20px;
        display: flex;
        flex-direction: column;
        gap: 16px;
        box-shadow: 0 2px 8px rgba(0,0,0,0.05);
    }

    .clues-container {
        flex: 1;
        display: flex;
        flex-direction: column;
        gap: 12px;
        font-size: 15px;
        color: #333;
    }

    .clue-item {
        padding: 8px 12px;
        border-radius: 8px;
        background-color: #fafafa;
        border: 1px solid #eee;
        text-align: left;
    }


    .button-group {
        display: flex;
        justify-content: center; /* 按钮居中 */
        gap: 12px; /* 按钮间距 */
        margin-top: auto; /* 贴底 */
    }

        .button-group .el-button {
            padding: 8px 16px;
            font-size: 16px;
            border-radius: 10px;
        }


    .right-section {
        flex: 1;
        background: #fff;
        border-radius: 16px;
        padding: 10px;
        box-shadow: 0 2px 8px rgba(0,0,0,0.05);
        display: flex;
        flex-direction: column; /* 纵向排列：标题在上，棋盘在下 */
    }

        .right-section h3 {
            text-align: center; /* 标题居中 */
            margin-bottom: 12px;
        }

    .grid-container {
        flex: 1; /* 占满剩余空间 */
        display: flex;
        justify-content: center; /* 水平居中 */
        align-items: center; /* 垂直居中 */
    }


    .grid {
        display: inline-flex;
        flex-direction: column;
        border: 2px solid #409EFF;
        border-radius: 8px;
        overflow: hidden;
    }

    .grid-row {
        display: flex;
    }

    .grid-cell {
        width: 40px;
        height: 40px;
        border: 1px solid #ddd;
        display: flex;
        align-items: center;
        justify-content: center;
    }

    .black-cell {
        background-color: #333;
    }

    .grid-input {
        width: 100%;
        height: 100%;
        border: none;
        text-align: center;
        font-size: 18px;
        text-transform: uppercase;
        outline: none;
    }

        .grid-input:focus {
            background-color: #e6f7ff;
        }

    .left-section h3,
    .right-section h3 {
        color: #000;
        margin-bottom: 12px;
    }

    .correct {
        background-color: #d9f7be !important;
    }

    .wrong {
        background-color: #ffd6e7 !important;
    }
</style>
