export interface Word {
  id: number;
  text: string;
  pronunciation: string;
  translation: string;
  examType: string;
  example: string;
  learned: boolean;
  bookmarked: boolean;
  errorCount: number;
  lastReviewed?: string;
}

export interface Option {
  text: string;
  isCorrect: boolean;
}

export interface ApiWordResponse {
  wordName: string;
  // englishDefinitions: string[];
  chineseTranslations: string[];
}


