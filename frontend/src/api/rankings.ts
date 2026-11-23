import { apiClient } from "./client";
import type { RankingEntryDto } from "../types";

export const rankingsApi = {
  getAll: () => apiClient.get<RankingEntryDto[]>("/rankings"),
};
