import { useState, useEffect } from 'react'
import axios from 'axios'
import { v4 as uuidv4 } from 'uuid'
import { PlusIcon, TrashIcon, CheckCircleIcon, RocketLaunchIcon } from '@heroicons/react/24/outline'

// API Base URL - Adjust if your backend port is different
const API_BASE = 'http://localhost:5136/api/goals'

interface Goal {
  userId: string
  goalId: string
  title: string
  description: string
  isCompleted: boolean
  createdAt: string
}

function App() {
  const [goals, setGoals] = useState<Goal[]>([])
  const [newGoal, setNewGoal] = useState('')
  const [userId, setUserId] = useState('')
  const [loading, setLoading] = useState(true)

  // Initialize Anonymous Session
  useEffect(() => {
    let id = localStorage.getItem('goalnexus_user_id')
    if (!id) {
      id = uuidv4()
      localStorage.setItem('goalnexus_user_id', id)
    }
    setUserId(id)
    fetchGoals(id)
  }, [])

  const fetchGoals = async (uid: string) => {
    try {
      const response = await axios.get(`${API_BASE}/${uid}`)
      setGoals(response.data)
    } catch (error) {
      console.error('Error fetching goals:', error)
    } finally {
      setLoading(false)
    }
  }

  const addGoal = async () => {
    if (!newGoal.trim()) return
    try {
      const response = await axios.post(API_BASE, {
        userId: userId,
        title: newGoal,
        description: "",
      })
      setGoals([...goals, response.data])
      setNewGoal('')
    } catch (error) {
      console.error('Error adding goal:', error)
    }
  }

  const toggleGoal = async (goalId: string) => {
    try {
      await axios.patch(`${API_BASE}/${userId}/${goalId}/toggle`)
      setGoals(goals.map(g => 
        g.goalId === goalId ? { ...g, isCompleted: !g.isCompleted } : g
      ))
    } catch (error) {
      console.error('Error toggling goal:', error)
    }
  }

  const deleteGoal = async (goalId: string) => {
    try {
      await axios.delete(`${API_BASE}/${userId}/${goalId}`)
      setGoals(goals.filter(g => g.goalId !== goalId))
    } catch (error) {
      console.error('Error deleting goal:', error)
    }
  }

  return (
    <div className="min-h-screen w-full bg-slate-950 text-slate-200 px-4 py-12 font-sans">
      <div className="max-w-2xl mx-auto">
        {/* Header */}
        <header className="mb-12 text-center">
          <div className="inline-flex items-center justify-center p-3 bg-indigo-500/10 rounded-2xl mb-4 border border-indigo-500/20">
            <RocketLaunchIcon className="w-8 h-8 text-indigo-400" />
          </div>
          <h1 className="text-4xl font-bold bg-gradient-to-r from-white to-slate-400 bg-clip-text text-transparent mb-2">
            GoalNexus
          </h1>
          <p className="text-slate-400">Simple, Anonymous, Secure. Your goals, your browser.</p>
          <div className="mt-2 text-xs font-mono text-slate-600 truncate">
            Session: {userId}
          </div>
        </header>

        {/* Input Area */}
        <div className="relative mb-8 group">
          <input
            type="text"
            value={newGoal}
            onChange={(e) => setNewGoal(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && addGoal()}
            placeholder="What's your next move?"
            className="w-full bg-slate-900/50 border border-slate-800 rounded-2xl py-4 pl-6 pr-16 focus:outline-none focus:ring-2 focus:ring-indigo-500/50 transition-all placeholder:text-slate-600"
          />
          <button
            onClick={addGoal}
            className="absolute right-2 top-2 p-2 bg-indigo-600 hover:bg-indigo-500 text-white rounded-xl transition-colors shadow-lg shadow-indigo-500/20"
          >
            <PlusIcon className="w-6 h-6" />
          </button>
        </div>

        {/* List Area */}
        <div className="space-y-3">
          {loading ? (
            <div className="flex justify-center py-12">
              <div className="animate-spin rounded-full h-8 w-8 border-t-2 border-b-2 border-indigo-500"></div>
            </div>
          ) : goals.length === 0 ? (
            <div className="text-center py-20 border-2 border-dashed border-slate-900 rounded-3xl">
              <p className="text-slate-500">No goals yet. Start small, win big.</p>
            </div>
          ) : (
            goals.map((goal) => (
              <div
                key={goal.goalId}
                className={`group flex items-center justify-between p-4 bg-slate-900/40 border border-slate-800/50 rounded-2xl hover:border-slate-700 transition-all ${
                  goal.isCompleted ? 'opacity-50' : ''
                }`}
              >
                <div className="flex items-center gap-4">
                  <button
                    onClick={() => toggleGoal(goal.goalId)}
                    className={`w-6 h-6 rounded-full border-2 flex items-center justify-center transition-colors ${
                      goal.isCompleted 
                        ? 'bg-indigo-500 border-indigo-500' 
                        : 'border-slate-700 hover:border-indigo-500'
                    }`}
                  >
                    {goal.isCompleted && <CheckCircleIcon className="w-5 h-5 text-white" />}
                  </button>
                  <span className={`text-lg font-medium transition-all ${
                    goal.isCompleted ? 'line-through text-slate-500' : 'text-slate-200'
                  }`}>
                    {goal.title}
                  </span>
                </div>
                
                <button
                  onClick={() => deleteGoal(goal.goalId)}
                  className="p-2 text-slate-600 hover:text-red-400 hover:bg-red-400/10 rounded-lg transition-all opacity-0 group-hover:opacity-100"
                >
                  <TrashIcon className="w-5 h-5" />
                </button>
              </div>
            ))
          )}
        </div>

        {/* Footer Info */}
        <footer className="mt-16 pt-8 border-t border-slate-900 text-center text-xs text-slate-600 space-y-2">
          <p>Built with ASP.NET Core Minimal API, React, Tailwind & AWS DynamoDB</p>
          <p>Your "Secret ID" is stored locally in your browser to keep your list private without an account.</p>
        </footer>
      </div>
    </div>
  )
}

export default App
