using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AllEnBackend.Models
{
    public class Vocabulary
    {
        public int Id { get; set; } // 单词的唯一标识符
        public string WordName { get; set; } = string.Empty;
        public string Coverage { get; set; } = string.Empty; // 新增字段

        // 导航属性
        public List<Definition> Definitions { get; set; } = new();
        public List<Translation> Translations { get; set; } = new();
        public List<Phonetic> Phonetics { get; set; } = new();
        public List<Example> Examples { get; set; } = new();
    }

    public class Definition
    {
        public int Id { get; set; } // 定义项的唯一标识符
        public string EnglishDefinition { get; set; } = string.Empty;
        public Vocabulary Vocabulary { get; set; } = null!; // 只有导航属性
    }

    public class Translation
    {
        public int Id { get; set; } // 翻译项的唯一标识符
        public string ChineseTranslation { get; set; } = string.Empty;
        public Vocabulary Vocabulary { get; set; } = null!; // 只有导航属性
    }

    public class UserWord
    {
        [Column("USER_ID")]
        public string UserId { get; set; } = string.Empty; // 关联用户的ID

        [Column("WORD_ID")]
        public int WordId { get; set; } // 关联单词的ID

        [Column("HAS_LEARNED")]
        public int HasLearned { get; set; } = 0; // 标记是否已学习(0/1)

        [Column("CORRECT_COUNT")]
        public int CorrectCount { get; set; } = 0; // 正确回答次数统计

        [Column("LEARN_COUNT")]
        public int LearnCount { get; set; } = 0; // 学习次数统计

        [Column("HAS_BOOKMARKED")]
        public int HasBookmarked { get; set; } = 0; // 标记是否已收藏(0/1)

        // 导航属性
        public Vocabulary Vocabulary { get; set; } = null!;
    }

    public class Phonetic
    {
        public int Id { get; set; } // 音标项的唯一标识符
        public string PhoneticText { get; set; } = string.Empty;
        public Vocabulary Vocabulary { get; set; } = null!;
    }

    public class Example
    {
        public int Id { get; set; } // 例句项的唯一标识符
        public string ExampleText { get; set; } = string.Empty;
        public Vocabulary Vocabulary { get; set; } = null!;
    }
}