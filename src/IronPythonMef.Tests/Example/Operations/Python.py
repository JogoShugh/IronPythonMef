@export(IOperation)
class Fibonacci(IOperation):
    def Execute(self, n):
        n = int(n)
        if n == 0:
            return 0
        elif n == 1:
            return 1
        else:
            return self.Execute(n-1) + self.Execute(n-2)
    
    @property
    def Name(self):
        return "fib"

    @property
    def Usage(self):
        return "fib n -- calculates the nth Fibonacci number"

@export(IOperation)
class Absolute(IOperation):
    def Execute(self, n):
        n = float(n)
        if (n < 0):
            return -n
        return n
    
    @property
    def Name(self):
        return "abs"

    @property
    def Usage(self):
        return "abs f -- calculates the absolute value of f"

@export(IOperation)
class Circumference(IOperation):
    @import_one(IMathCheatSheet)
    def import_cheatSheet(self, cheatSheet):
        self.cheatSheet = cheatSheet

    def Execute(self, d):
        d = float(d)
        return self.cheatSheet.Pi * d

    @property
    def Name(self):
        return "crc"

    @property 
    def USage(self):
        return "crc d -- calculaets the circumference of a circle with diameter d"